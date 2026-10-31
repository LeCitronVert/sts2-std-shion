using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Always Novel. Apply 6 Confusion. Gain 4 Block for each Flashback. Lose all Flashback.
/// </summary>
public class SlalCartoonyDodge() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Confusion>(6),
        new CalculationBaseVar(0),
        new CalculationExtraVar(4),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, _) => SlalMechanicsHelper.GetFlashback(card.Owner.Creature)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromPower<Flashback>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);

        if (SlalMechanicsHelper.GetFlashback(card.Owner.Creature) <= 0) return;
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.CalculatedBlock.Calculate(play.Target), card.DynamicVars.CalculatedBlock.Props, play);
        await SlalMechanicsHelper.LoseAllFlashback(card.Owner.Creature);
    }

    protected override void OnUpgrade()
    {
        //todo : upgrade not specified
        EnergyCost.UpgradeBy(-1);
    }
}
