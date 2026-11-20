using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Powerful and Helpless (Team-Up), on Fry. Amount = 1, or 2 when upgraded (stacks add up).
/// Partners can't Patch Up (ShionMechanicsHelper.PatchUpCreature) nor revive (ComeWhatMayDeathPatch), but deal 50% more Attack damage per Amount.
/// Fry's Requests don't trigger their effects: they give 2 Strength and Dexterity per Amount instead (Requested.Advance).
/// </summary>
public class PowerfulAndHelpless : TeamUpPower
{
    public const int DamagePercentPerStack = 50;
    public const int StatsPerStack = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("PercentPerStack", DamagePercentPerStack),
        new DynamicVar("StatsPerStack", StatsPerStack),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    /// <summary>Whether a Powerful and Helpless links this creature as a Partner.</summary>
    public static bool IsHelpless(Creature creature)
    {
        return creature.CombatState?.Allies
            .Any(c => c.GetPower<PowerfulAndHelpless>() is { } power && power.Partners.Contains(creature)) ?? false;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
        , CardPlay? cardPlay
    )
    {
        if (dealer is null || !Partners.Contains(dealer) || !props.IsPoweredAttack()) return 1M;
        return 1M + DamagePercentPerStack * Amount / 100M;
    }

    /// <summary>
    /// Called when one of the owner's Requests completes: if they have this power, it gives Strength and Dexterity
    /// instead of its effect. Returns whether the effect was replaced.
    /// </summary>
    public static async Task<bool> TryReplaceRequest(PlayerChoiceContext choiceContext, Creature owner)
    {
        if (owner.GetPower<PowerfulAndHelpless>() is not { } power) return false;

        power.Flash();
        var stats = StatsPerStack * power.Amount;
        await PowerCmd.Apply<StrengthPower>(choiceContext, owner, stats, owner, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, owner, stats, owner, null);
        return true;
    }
}
