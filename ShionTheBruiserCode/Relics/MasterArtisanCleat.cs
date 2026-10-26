using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class MasterArtisanCleat() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;
    
    public override bool HasUponPickupEffect => true;
    
    public override async Task AfterObtained()
    {
        var relic = this;
        var list = ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(
            relic.Owner.UnlockState,
            relic.Owner.RunState.CardMultiplayerConstraint
        );
        var count = list.Count();
        
        var cardGeneration = CardFactory.CreateForReward(
            relic.Owner,
            count,
            new CardCreationOptions(list, CardCreationSource.Other, CardRarityOddsType.Uniform)
        ).Where((result => result.Card.CanBeGeneratedInCombat)).ToList();

        var card = (await CardSelectCmd.FromSimpleGridForRewards(
            new BlockingPlayerChoiceContext(),
            cardGeneration,
            Owner,
            new CardSelectorPrefs(new LocString("relics", "SHIONTHEBRUISER-MASTER_ARTISAN_CLEAT.selection"), 1)
        )).First();

        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
    }
}