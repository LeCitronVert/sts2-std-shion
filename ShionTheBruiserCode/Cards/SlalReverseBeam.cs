using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Add Incoming Beam (+) into your Draw Pile. Add a copy of each card in your Hand into your Discard Pile.
/// Incoming Beam is added first, so the copies increase its damage.
/// </summary>
public class SlalReverseBeam() : SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalIncomingBeam>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
            SlalMechanicsHelper.CreateCard<SlalIncomingBeam>(card.Owner, IsUpgraded), PileType.Draw, card.Owner, CardPilePosition.Random));

        var copies = PileType.Hand.GetPile(card.Owner).Cards.Select(c => c.CreateClone()).ToList();
        if (copies.Count == 0) return;
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Discard, card.Owner));
    }

    protected override void OnUpgrade()
    {
    }
}
