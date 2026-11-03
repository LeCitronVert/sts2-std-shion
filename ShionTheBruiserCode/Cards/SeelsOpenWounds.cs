using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// X-cost. Gain X energy. Draw X (X+1) cards. Add X Wounds into your Discard Pile.
/// </summary>
public class SeelsOpenWounds() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ExtraDraw", 0M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("XAmount").WithMultiplier((card, _) => PreviewHelper.XValue(card)),
        new CalculatedVar("DrawAmount").WithMultiplier((card, _) =>
            PreviewHelper.XValue(card) + card.DynamicVars["ExtraDraw"].IntValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.ForEnergy(this),
        HoverTipFactory.FromCard<Wound>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var x = ResolveEnergyXValue();

        if (x > 0) await PlayerCmd.GainEnergy(x, Owner);
        var draw = x + DynamicVars["ExtraDraw"].IntValue;
        if (draw > 0) await CardPileCmd.Draw(choiceContext, draw, Owner);
        await SeelsMechanicsHelper.AddWounds(Owner, x, PileType.Discard);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ExtraDraw"].UpgradeValueBy(1M);
    }
}
