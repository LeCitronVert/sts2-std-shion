using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionBorrowTechnique() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyAlly)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ogCard = this;
        var target = play.Target;
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(target.Player);
        
        CardModel? card = CardFactory.GetDistinctForCombat(
            ogCard.Owner, 
            target.Player.Character.CardPool.GetUnlockedCards(
                    target.Player.UnlockState, 
                    target.Player.RunState.CardMultiplayerConstraint
                )
                .Where<CardModel>(
                    (Func<CardModel, bool>) (c => c.Type == CardType.Skill)), 
            1, 
            ogCard.Owner.RunState.Rng.CombatCardGeneration
        ).FirstOrDefault<CardModel>();
        if (card == null)
            return;
        card.SetToFreeThisTurn();
        CardPileAddResult combat = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, ogCard.Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}