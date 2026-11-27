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
/// At the start of each fight, enter <see cref="SuperFormPower"/>.
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

    public static bool Has(Player? player) => player?.Relics.Any(r => r is SuperForm) == true;
}

/// <summary>
/// Lets you turn your <see cref="Rings"/>' damage protection off and back on from the Emerald wheel
/// (<see cref="EmeraldWheel"/>), visuals included. Without Super Form, Rings always protect.
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

    /// <summary>Whether this creature is in Super Form with its Ring protection on (aura visuals).</summary>
    public static bool IsProtecting(Creature creature) =>
        creature.IsAlive && creature.GetPower<SuperFormPower>() is { ProtectionOn: true };

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
