using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergySpark() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new PowerVar<VigorPower>(1),
        new CriticalDrawVar(5)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>()
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionEnergySpark source = this;
        int cardCount = source.DynamicVars.Cards.IntValue;
        await CreatureCmd.TriggerAnim(
            source.Owner.Creature,
            Character.ShionTheBruiser.GetEnergyCastAnimIfApplicable(source.Owner.Character),
            Character.ShionTheBruiser.GetEnergyCastDelayIfApplicable(source.Owner.Character)
        );
        await CardPileCmd.Draw(choiceContext, cardCount, source.Owner);
        await PowerCmd.Apply<VigorPower>(choiceContext, source.Owner.Creature, source.DynamicVars["VigorPower"].IntValue, source.Owner.Creature, source);
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.Cards.UpgradeValueBy(1M);
        this.DynamicVars["VigorPower"].UpgradeValueBy(1M);
    }
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
    }
}