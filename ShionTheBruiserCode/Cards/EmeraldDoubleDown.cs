using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Utils;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Green Chaos Emerald card: apply the last debuff the enemy received again, as many times as the enemy's Weak.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldDoubleDown() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Token,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HasTargetVar(),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("DoubleDownAmount").WithMultiplier((_, target) =>
            target is null ? 0 : Math.Abs(AmountToApply(target))),
        new DoubleDownPowerVar(),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var lastDebuff = LastDebuffReceived(play.Target);
        var amount = AmountToApply(play.Target);
        if (lastDebuff is null || amount == 0) return;

        await PowerCmd.Apply(choiceContext, lastDebuff.ToMutable(), play.Target, amount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    /// <summary>
    /// Signed amount to apply: the target's Weak, negative when the last debuff was a Strength/Dexterity-style reduction.
    /// </summary>
    private static decimal AmountToApply(Creature target)
    {
        var entry = LastDebuffEntry(target);
        if (entry is null) return 0;
        return target.GetPowerAmount<WeakPower>() * Math.Sign(entry.Amount);
    }

    private static PowerModel? LastDebuffReceived(Creature target)
    {
        var entry = LastDebuffEntry(target);
        return entry is null ? null : ModelDb.GetById<PowerModel>(entry.Power.Id);
    }

    /// <summary>
    /// Display-only var: the name of the debuff that will be applied to the hovered enemy.
    /// </summary>
    private class DoubleDownPowerVar() : StringVar("DoubleDownPower")
    {
        public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
        {
            var power = target is null ? null : LastDebuffReceived(target);
            if (power is null)
            {
                StringValue = "";
                return;
            }

            var title = $"[gold]{power.Title.GetFormattedText()}[/gold]";
            StringValue = AmountToApply(target!) < 0 ? $"negative {title}" : title;
        }
    }

    private static PowerReceivedEntry? LastDebuffEntry(Creature target)
    {
        if (!CombatManager.Instance.IsInProgress) return null;
        return CombatManager.Instance.History.Entries
            .OfType<PowerReceivedEntry>()
            .LastOrDefault(e => e.Actor == target && IsDebuffApplication(e));
    }

    private static bool IsDebuffApplication(PowerReceivedEntry entry)
    {
        var canonical = ModelDb.GetById<PowerModel>(entry.Power.Id);
        if (canonical is EmeraldChallengePower) return false;
        return canonical.Type switch
        {
            PowerType.Debuff => entry.Amount > 0,
            // Negative Strength/Dexterity etc. are debuffs too
            PowerType.Buff => entry.Amount < 0 && canonical.AllowNegative,
            _ => false,
        };
    }
}
