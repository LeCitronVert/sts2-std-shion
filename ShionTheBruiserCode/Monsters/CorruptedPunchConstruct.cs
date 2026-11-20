using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Monsters;

/// <summary>
/// SkeleBot 9000's pet: the Punch Construct enemy's model with a gray filter, facing the enemies.
/// Takes attack damage for its owner and acts at the end of their turn (see <see cref="CorruptedPunchConstructPower"/>).
/// </summary>
public class CorruptedPunchConstruct : CustomMonsterModel
{
    private static readonly string ConstructVisualsPath = SceneHelper.GetScenePath("creature_visuals/punch_construct");

    private static readonly Color CorruptedTint = new(0.55f, 0.55f, 0.6f);

    public override int MinInitialHp => 1;

    public override int MaxInitialHp => 1;

#if STS2_BETA
    public override bool IsInteractable => Creature.IsAlive;
#else
    public override bool IsHealthBarVisible => Creature.IsAlive;
#endif

    public override string? CustomVisualPath => ConstructVisualsPath;

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

    /// <summary>
    /// Summons a Corrupted Punch Construct for the player with this much Max HP.
    /// If they already have a living one, it gains that much Max HP instead.
    /// </summary>
    public static async Task Summon(PlayerChoiceContext choiceContext, Player player, decimal maxHp, CardModel? source)
    {
        if (player.PlayerCombatState is not { } combatState || player.Creature.IsDead) return;
        maxHp = Math.Max(1m, maxHp);

        var construct = GetConstruct(player);
        if (construct != null)
        {
            await CreatureCmd.GainMaxHp(construct, maxHp);
            return;
        }

        construct = await PlayerCmd.AddPet<CorruptedPunchConstruct>(player);
        // The combat room makes every pet but Osty non-interactable (no hover, no health bar), like Byrdpip
#if STS2_BETA
        NCombatRoom.Instance?.GetCreatureNode(construct)?.SetIsInteractable(true, true);
#else
        NCombatRoom.Instance?.SetCreatureIsInteractable(construct, true);
#endif
        await PowerCmd.Apply<CorruptedPunchConstructPower>(choiceContext, construct, 1, player.Creature, source);
        await CreatureCmd.SetMaxHp(construct, maxHp);
        await CreatureCmd.Heal(construct, maxHp, false);
    }

    public static Creature? GetConstruct(Player player)
    {
        return player.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster is CorruptedPunchConstruct && p.IsAlive);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var initialState = new MoveState("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);
        initialState.FollowUpState = initialState;
        return new MonsterMoveStateMachine([initialState], initialState);
    }

    public override NCreatureVisuals CreateCustomVisuals()
    {
        // Loaded directly: it's summoned mid-combat, so it may not be in the preload cache
        var visuals = ResourceLoader.Load<PackedScene>(ConstructVisualsPath, null, ResourceLoader.CacheMode.Reuse)
            .Instantiate<NCreatureVisuals>(PackedScene.GenEditState.Disabled);
        visuals.Modulate = CorruptedTint;

        // Punch Constructs face the players: flip the body so it faces the enemies
        if (visuals.GetNodeOrNull<Node2D>("%Visuals") is { } body)
        {
            body.Scale = new Vector2(-body.Scale.X, body.Scale.Y);
        }

        return visuals;
    }
}
