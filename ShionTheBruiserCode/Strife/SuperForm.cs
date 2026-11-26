using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Granted to every player by the Chaos Emeralds at the end of Act 3 (<see cref="StrifeTransition"/>).
/// At the start of each fight, enter <see cref="SuperFormPower"/>. Your Gold is shown as Rings (cosmetic, see
/// <see cref="RingVisuals"/>).
/// </summary>
[Pool(typeof(EventRelicPool))]
public class SuperForm : ShionTheBruiserRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => "super_form.png".RelicImagePath();
    // No outline drawn yet (MISSING_IMAGES.tsv)
    protected override string PackedIconOutlinePath => "relic_outline.png".RelicImagePath();
    protected override string BigIconPath => PackedIconPath;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SuperFormPower>()];

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<SuperFormPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, null);
    }

    public override Task AfterObtained()
    {
        RingVisuals.RefreshGoldIcon();
        return Task.CompletedTask;
    }

    public static bool Has(Player? player) => player?.Relics.Any(r => r is SuperForm) == true;
}

/// <summary>
/// Unblocked damage you take is absorbed by your Rings (Gold) first, 1 Ring per point, until you run out. That protection
/// can be turned off and back on from the Emerald wheel (<see cref="EmeraldWheel"/>), visuals included.
/// Damage you deal yourself isn't absorbed.
/// Also holds this fight's Emerald wheel state: each Emerald Power can be picked once per fight. Whenever an enemy dies,
/// one used Emerald Power (at random) can be picked again, and if the Team Blast gauge was used this combat, it's unlocked
/// and back to 0 for the whole team.
/// Visuals (aura, sparks, sweat, Ring health bar segment, music): <see cref="SuperFormFx"/>, <see cref="RingVisuals"/>.
/// </summary>
public sealed class SuperFormPower : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // Same icon as the Super Form relic
    public override string CustomPackedIconPath => "super_form.png".RelicImagePath();
    public override string CustomBigIconPath => "super_form.png".RelicImagePath();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(DreamersKeywords.EmeraldPower)];

    private int _pendingRingLoss;

    private int Rings => Owner.Player?.Gold ?? 0;

    // ── Emerald wheel state (only changed by EmeraldWheelAction and hooks: the same on every client) ──

    /// <summary>Whether the Rings absorb damage (toggled from the Emerald wheel).</summary>
    public bool ProtectionOn { get; private set; } = true;

    // Created on first use: a field initializer would be shared with the canonical model by MutableClone
    private HashSet<ChaosEmeraldColor>? _usedEmeralds;

    public bool IsUsed(ChaosEmeraldColor color) => _usedEmeralds?.Contains(color) == true;

    public void MarkUsed(ChaosEmeraldColor color)
    {
        (_usedEmeralds ??= []).Add(color);
        Flash();
    }

    public void ToggleProtection()
    {
        ProtectionOn = !ProtectionOn;
        Flash();
    }

    /// <summary>Whether this creature's Rings absorb damage right now (logic and visuals).</summary>
    public static bool IsProtecting(Creature creature) =>
        creature.IsAlive && creature.GetPower<SuperFormPower>() is { ProtectionOn: true };

    // Only called by real damage (not by previews), so tracking the absorbed amount here is safe
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || amount <= 0 || dealer == Owner || Owner.Player == null || !ProtectionOn) return amount;
        var available = Rings - _pendingRingLoss;
        if (available <= 0) return amount;

        var absorbed = (int)Math.Min(available, Math.Ceiling(amount));
        _pendingRingLoss += absorbed;
        return Math.Max(0m, amount - absorbed);
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        if (_pendingRingLoss <= 0 || Owner.Player is not { } player) return;
        var loss = _pendingRingLoss;
        _pendingRingLoss = 0;
        Flash();
        await PlayerCmd.LoseGold(loss, player, GoldLossType.Lost);
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature,
#if STS2_BETA
        CardModel? source,
#endif
        bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature.Side == Owner.Side || Owner.IsDead || Owner.Player is not { } player) return Task.CompletedTask;

        // A used Team Blast gauge is unlocked and back to 0 for the whole team
        if (Singletons.TeamBlastSingleton.Instance?.ResetUsedGauge() == true) Flash();

        // One used Emerald Power (at random) can be picked again. Sorted first: the same pick on every client
        var used = ChaosEmeraldHelper.AllColors.Where(IsUsed).ToList();
        if (used.Count == 0) return Task.CompletedTask;
        var refreshed = player.RunState.Rng.Niche.NextItem(used);
        _usedEmeralds!.Remove(refreshed);
        Flash();
        return Task.CompletedTask;
    }
}
