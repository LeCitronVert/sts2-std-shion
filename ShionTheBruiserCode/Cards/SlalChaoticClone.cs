using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 6 Block. Play a random Slimed in your Hand. If you do, choose to Mimic 2 or Shapeshift
/// (upgraded: and upgrade that card, i.e. the offered cards are upgraded).
/// Not written: when Flashback plays it, it always Shapeshifts.
/// </summary>
public class SlalChaoticClone() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6M, ValueProp.Move),
        new CardsVar(2),
    ];

    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Shapeshift, SlalCardTags.Mimic];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Mimic),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Shapeshift),
        HoverTipFactory.FromKeyword(SlalCardKeywords.GoofballTokenPower),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);

        var slimed = PileType.Hand.GetPile(card.Owner).Cards
            .Where(c => c is Slimed)
            .TakeRandom(1, card.Owner.RunState.Rng.CombatCardSelection)
            .FirstOrDefault();
        if (slimed == null) return;

        await CardCmd.AutoPlay(choiceContext, slimed, null);
        await SlalMechanicsHelper.MimicOrShapeshift(choiceContext, card.Owner, card.DynamicVars.Cards.IntValue, IsUpgraded, card);
    }

    protected override void OnUpgrade()
    {
    }
}
