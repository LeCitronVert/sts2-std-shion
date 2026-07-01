using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionHalfHearted() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var player = card.Owner;
        IReadOnlyList<CardPileAddResult> result = await CardPileCmd.AddGeneratedCardsToCombat(CardFactory.GetDistinctForCombat(player, ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint), 1, player.RunState.Rng.CombatCardGeneration).ToList(), PileType.Hand, card.Owner);
        var addedCard = result.First().cardAdded;
        addedCard.AddKeyword(CardKeyword.Ethereal);
        if (card.IsUpgraded)
        {
            CardCmd.Upgrade(addedCard);
        }
    }

    protected override void OnUpgrade()
    {
    }
}