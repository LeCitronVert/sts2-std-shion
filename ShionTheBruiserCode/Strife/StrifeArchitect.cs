using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using static ShionTheBruiser.ShionTheBruiserCode.Strife.StrifeConfig;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// The Act 4 Architect: the game's Architect sprite with an AI of summons and buffs.
/// <list type="bullet">
/// <item>No summon alive: summons the next line of <see cref="StrifeConfig.SummonCycle"/>.</item>
/// <item>Summons alive: goes through <see cref="StrifeConfig.BuffCycle"/> (buffs reach his summons, see <see cref="ArchitectsDesign"/>).</item>
/// <item>Once his summons are all dead: if he summoned twice since his last attack, he says a line (unknown intent), then
/// attacks for the combined Max HP of the party. Once the whole cycle was summoned (and that attack done), he is
/// "defeated". Otherwise he summons the next line.</item>
/// </list>
/// </summary>
public sealed class StrifeArchitect : CustomMonsterModel
{
    private const string LocPrefix = "SHIONTHEBRUISER-STRIFE_ARCHITECT";

    public const int InfiniteHp = 999999;

    public override int MinInitialHp => InfiniteHp;
    public override int MaxInitialHp => InfiniteHp;

    public override string? CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/architect");

    public override bool ShouldDisappearFromDoom => false;

    // ── Fight state (deterministic, the same on every client) ───────────────

    /// <summary>Next line of the summon cycle.</summary>
    public int NextSummonIndex { get; private set; }

    /// <summary>Summon lines done since his last big attack (starts at 0).</summary>
    public int SummonsSinceAttack { get; private set; }

    public int BuffIndex { get; private set; }

    /// <summary>How many Intensify buffs he did.</summary>
    public int Tier { get; private set; }

    /// <summary>Strength his own buffs gave him (shared with summons that join later; debuffs don't count).</summary>
    public int SharedStrength { get; private set; }

    /// <summary>Damage taken this fight (his score).</summary>
    public long TotalDamage { get; private set; }

    private List<ModelId>? _summonedBosses;

    public event Action<StrifeArchitect>? DamageChanged;

    public void AddDamage(int amount)
    {
        if (amount <= 0) return;
        TotalDamage += amount;
        StrifeScores.CurrentRunDamage = TotalDamage;
        DamageChanged?.Invoke(this);
    }

    private ArchitectsDesign? Design => Creature.GetPower<ArchitectsDesign>();

    private bool AnySummonAlive => CombatState.GetTeammatesOf(Creature).Any(c => c != Creature && c.IsAlive);

    private bool CycleDone => NextSummonIndex >= SummonCycle.Count;

    private int PartyMaxHp
    {
        get
        {
            // The canonical model (Bestiary, previews outside of a fight) has no creature
            try { return CombatState.Players.Sum(p => p.Creature.MaxHp); }
            catch (InvalidOperationException) { return 0; }
            catch (NullReferenceException) { return 0; }
        }
    }

    // ── Moves ────────────────────────────────────────────────────────────────

    private MoveState _summon = null!;
    private MoveState _taunt = null!;
    private MoveState _bigAttack = null!;
    private MoveState _defeated = null!;
    private Dictionary<BuffKind, MoveState> _buffs = null!;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        _summon = new MoveState("SUMMON", SummonMove, new SummonIntent());
        _taunt = new MoveState("TAUNT", TauntMove, new UnknownIntent());
        _bigAttack = new MoveState("BIG_ATTACK", BigAttackMove, new SingleAttackIntent(() => PartyMaxHp));
        _defeated = new MoveState("DEFEATED", DefeatedMove, new UnknownIntent());
        _buffs = new Dictionary<BuffKind, MoveState>
        {
            [BuffKind.Strength] = new("BUFF_STRENGTH", _ => BuffMove(BuffKind.Strength), new BuffIntent()),
            [BuffKind.Block] = new("BUFF_BLOCK", _ => BuffMove(BuffKind.Block), new DefendIntent()),
            [BuffKind.Vigor] = new("BUFF_VIGOR", _ => BuffMove(BuffKind.Vigor), new BuffIntent()),
            [BuffKind.HealPercent] = new("BUFF_HEAL", _ => BuffMove(BuffKind.HealPercent), new HealIntent()),
            [BuffKind.Intensify] = new("BUFF_INTENSIFY", _ => BuffMove(BuffKind.Intensify), new BuffIntent()),
            [BuffKind.Impatience] = new("BUFF_IMPATIENCE", _ => BuffMove(BuffKind.Impatience), new DebuffIntent()),
        };

        var decide = new ConditionalBranchState("DECIDE");
        List<MonsterState> states = [_summon, _taunt, _bigAttack, _defeated, .. _buffs.Values, decide];
        foreach (var move in states.OfType<MoveState>())
        {
            var captured = move;
            decide.AddState(move, () => Decide() == captured);
            move.FollowUpState = decide;
        }
        // He always attacks right after his line
        _taunt.FollowUpState = _bigAttack;
        _defeated.FollowUpState = _defeated;

        return new MonsterMoveStateMachine(states, _summon);
    }

    private MoveState Decide()
    {
        if (AnySummonAlive) return _buffs[BuffCycle[BuffIndex % BuffCycle.Count]];
        if (SummonsSinceAttack >= SummonsPerBigAttack) return _taunt;
        if (CycleDone) return _defeated;
        return _summon;
    }

    /// <summary>When his last summon dies, his intent changes right away (big attack line, summon or defeat).</summary>
    public void OnSummonDied()
    {
        if (AnySummonAlive || !_buffs.ContainsValue(NextMove)) return;
        SetMoveImmediate(Decide());
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        StrifeLayout.Reset();
        StrifeScores.CurrentRunDamage = 0;
        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<ArchitectsDesign>(context, Creature, 1m, Creature, null);
        await PowerCmd.Apply<FieldExperiment>(context, Creature, 1m, Creature, null);
        await PowerCmd.Apply<MakeYourPoint>(context, Creature, 1m, Creature, null);
        await PowerCmd.Apply<GrowingImpatience>(context, Creature, StartingCardLimit, Creature, null);
        Creature.HpDisplay = HpDisplay.InfiniteWithoutNumbers;
    }

    private async Task SummonMove(IReadOnlyList<Creature> _)
    {
        if (CycleDone) return;
        var entry = SummonCycle[NextSummonIndex];
        NextSummonIndex++;
        SummonsSinceAttack++;

        await CreatureCmd.TriggerAnim(Creature, "Attack", 0.3f);
        if (!CombatState.IsLiveCombat()) return;

        var encounters = entry.UnfoughtBossOfActIndex is { } actIndex
            ? [PickUnfoughtBoss(actIndex)]
            : entry.Encounters.Select(e => e()).ToList();

        for (var index = 0; index < encounters.Count; index++)
        {
            var encounter = encounters[index].ToMutable();
            encounter.GenerateMonstersWithSlots(CombatState.RunState);
            StrifeLayout.BeginGroup(encounter, index, encounters.Count);

            var design = Design;
            if (design != null) design.PendingSummonType = encounter.RoomType;
            try
            {
                foreach (var (monster, slot) in encounter.MonstersWithSlots)
                {
                    await CreatureCmd.Add(monster, CombatState, CombatSide.Enemy, slot);
                }
            }
            finally
            {
                if (design != null) design.PendingSummonType = null;
                StrifeLayout.EndGroup();
            }
        }
    }

    /// <summary>
    /// A boss of that act that wasn't fought this run (nor already summoned in this fight), from the act the party visited.
    /// Falls back to the other variants of that act, then to any boss of the act.
    /// </summary>
    private EncounterModel PickUnfoughtBoss(int actIndex)
    {
        var runState = CombatState.RunState;
        var fought = runState.Acts.Take(3)
            .SelectMany(a => new[] { a.BossEncounter, a.HasSecondBoss ? a.SecondBossEncounter : null })
            .Where(e => e != null)
            .Select(e => e!.Id)
            .ToHashSet();
        _summonedBosses ??= [];
        var used = _summonedBosses;

        var visited = actIndex < runState.Acts.Count ? runState.Acts[actIndex].AllBossEncounters.ToList() : [];
        var variants = ActsOfIndex(actIndex).SelectMany(a => a.AllBossEncounters).ToList();

        var pick = Pick(visited.Where(e => !fought.Contains(e.Id) && !used.Contains(e.Id)))
                   ?? Pick(variants.Where(e => !fought.Contains(e.Id) && !used.Contains(e.Id)))
                   ?? Pick(visited.Where(e => !used.Contains(e.Id)))
                   ?? Pick(visited.Concat(variants))
                   ?? ModelDb.Encounter<MegaCrit.Sts2.Core.Models.Encounters.TheInsatiableBoss>();
        used.Add(pick.Id);
        return pick;

        EncounterModel? Pick(IEnumerable<EncounterModel> pool)
        {
            var list = pool.Where(CanBeSummoned).DistinctBy(e => e.Id).ToList();
            return list.Count == 0 ? null : RunRng.MonsterAi.NextItem(list);
        }
    }

    private async Task BuffMove(BuffKind kind)
    {
        BuffIndex++;
        var context = new ThrowingPlayerChoiceContext();
        var value = BuffValue(kind, Tier);
        var summons = CombatState.GetTeammatesOf(Creature).Where(c => c != Creature && c.IsAlive).ToList();
        await CreatureCmd.TriggerAnim(Creature, "Attack", 0.3f);

        switch (kind)
        {
            case BuffKind.Strength:
                // On him and every summon; later summons get SharedStrength when they join
                SharedStrength += value;
                await PowerCmd.Apply<StrengthPower>(context, Creature, value, Creature, null);
                foreach (var summon in summons) await PowerCmd.Apply<StrengthPower>(context, summon, value, Creature, null);
                break;
            case BuffKind.Vigor:
                await PowerCmd.Apply<VigorPower>(context, Creature, value, Creature, null);
                foreach (var summon in summons) await PowerCmd.Apply<VigorPower>(context, summon, value, Creature, null);
                break;
            case BuffKind.Block:
                foreach (var summon in summons) await CreatureCmd.GainBlock(summon, value, ValueProp.Move, null);
                break;
            case BuffKind.HealPercent:
                foreach (var summon in summons) await CreatureCmd.Heal(summon, Math.Ceiling(summon.MaxHp * value / 100m));
                break;
            case BuffKind.Intensify:
                Tier++;
                break;
            case BuffKind.Impatience:
                if (Creature.GetPower<GrowingImpatience>() is { } impatience && impatience.Amount > MinimumCardLimit)
                {
                    await PowerCmd.ModifyAmount(context, impatience, -1, Creature, null);
                }
                break;
        }
    }

    private async Task TauntMove(IReadOnlyList<Creature> _)
    {
        var line = RunRng.MonsterAi.NextInt(BigAttackLineCount);
        TalkCmd.Play(new LocString("monsters", $"{LocPrefix}.BIG_ATTACK_LINE.{line}"), Creature, VfxColor.DarkGray, VfxDuration.Long);
        await Cmd.CustomScaledWait(0.6f, 1.2f);
    }

    private async Task BigAttackMove(IReadOnlyList<Creature> _)
    {
        SummonsSinceAttack = 0;
        await DamageCmd.Attack(PartyMaxHp).FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_lightning")
            .Execute(null);
    }

    private async Task DefeatedMove(IReadOnlyList<Creature> _)
    {
        // Silence from here to the end of the run (the FMOD act music stays off too)
        StrifeMusic.Silenced = true;
        StrifeMusic.Refresh();
        TalkCmd.Play(new LocString("monsters", $"{LocPrefix}.DEFEATED.0"), Creature, VfxColor.Red, VfxDuration.Standard);
        await Cmd.Wait(2.2f);
        TalkCmd.Play(new LocString("monsters", $"{LocPrefix}.DEFEATED.1"), Creature, VfxColor.DarkGray, VfxDuration.Standard);
        await Cmd.Wait(2.2f);

        StrifeScores.MarkDefeated();
        if (Design is { } design) design.AllowDeath = true;
        await CreatureCmd.Kill(Creature, force: true);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        // Same states as the base game's Architect; he has no death animation, so he flinches instead
        var idle = new AnimState("idle_loop", isLooping: true);
        var attack = new AnimState("attack") { NextState = idle };
        var hurt = new AnimState("hurt") { NextState = idle };
        var dead = new AnimState("hurt");
        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", attack);
        animator.AddAnyState("Hit", hurt);
        animator.AddAnyState("Dead", dead);
        return animator;
    }
}

/// <summary>
/// The Act 4 boss fight: the Architect alone (he summons the rest). The Architect's victory room as background.
/// It's entered through <see cref="StrifeArchitectIntro"/>, which plays his opening lines first.
/// </summary>
public sealed class StrifeArchitectEncounter() : CustomEncounterModel(RoomType.Boss)
{
    private const string ArchitectSlot = "architect";

    public override bool IsValidForAct(ActModel act) => false; // StrifeAct lists it itself

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<StrifeArchitect>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [(ModelDb.Monster<StrifeArchitect>().ToMutable(), ArchitectSlot)];

    public override string? CustomScenePath => Path.Join(MainFile.ResPath, "scenes", "encounters", "strife_architect.tscn");

    /// <summary>The Architect plus the slots of the encounters he summoned (summons look for free slots in this list).</summary>
    public override IReadOnlyList<string> Slots => [ArchitectSlot, .. StrifeLayout.SummonedSlots];

    public override BackgroundAssets? CustomEncounterBackground(ActModel parentAct, MegaCrit.Sts2.Core.Random.Rng rng) =>
        new("the_architect_event_encounter", rng);

    public override string BossNodePath => Path.Join(MainFile.ResPath, "images", "map", "strife_architect_boss");

    // Placeholder art (MISSING_IMAGES.tsv)
    public override string? CustomRunHistoryIconPath => Path.Join(MainFile.ResPath, "images", "map", "strife_architect_boss.png");
    public override string? CustomRunHistoryIconOutlinePath => Path.Join(MainFile.ResPath, "images", "map", "strife_architect_boss_outline.png");

    // Rewards stay on: the rewards screen (empty after the last act's boss) is where "Proceed" leads to the ending
}
