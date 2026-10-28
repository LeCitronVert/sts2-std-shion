using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Transform a card in your Hand into a random non-Rare Colorless card (Rare allowed with Creative Spirit).
/// It keeps the same cost as the original card. An enemy loses Strength equal to that cost this turn.
/// The new card is upgraded if the original was, and Unplayable if the original was.
/// </summary>
public class SlalLogicShift() : SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        var original = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1),
            c => c.IsTransformable,
            card
        )).FirstOrDefault();
        if (original is null) return;

        // X-cost cards can't keep a cost, so they are left out
        var options = ModelDb.CardPool<ColorlessCardPool>()
            .GetUnlockedCards(card.Owner.UnlockState, card.Owner.RunState.CardMultiplayerConstraint)
            .Where(c => SlalMechanicsHelper.HasRandomCardRarity(card.Owner, c) && !c.EnergyCost.CostsX);
        var newCard = SlalMechanicsHelper.CreateRandomCards(card.Owner, 1, original.IsUpgraded, options).FirstOrDefault();
        if (newCard is null) return;

        var cost = SlalMechanicsHelper.GetEnergyCost(original);
        newCard.EnergyCost.SetThisCombat(cost);
        if (original.Keywords.Contains(CardKeyword.Unplayable))
        {
            newCard.AddKeyword(CardKeyword.Unplayable);
        }
        await CardCmd.Transform(original, newCard);

        if (cost > 0 && play.Target.IsAlive)
        {
            await PowerCmd.Apply<LogicShiftStrengthDown>(choiceContext, play.Target, cost, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
