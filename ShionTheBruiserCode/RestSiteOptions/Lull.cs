using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.RestSiteOptions;

public class Lull(Player owner) : CustomRestSiteOption(owner)
{
    public override LocString Description
    {
        get => new LocString("relics", "SHIONTHEBRUISER-MASTER_PENDANT.rest_desc");
    }

    public override string? CustomIconPath => Path.Join(MainFile.ResPath, "images", "ui", "rest", "lull.png");
    
    public override async Task<bool> OnSelect()
    {
        var cardList = new List<CardModel>
        {
            ModelDb.Card<CosmoKindness>(),
            ModelDb.Card<CosmoDetermination>(),
            ModelDb.Card<CosmoGuidance>(),
        };
        var cardResultList = CardFactory.CreateForReward(
            Owner,
            3,
            new CardCreationOptions(cardList, CardCreationSource.Other, CardRarityOddsType.Uniform)
        );
        var cards = new List<CardModel>();
        foreach (var cardCreationResult in cardResultList)
        {
            cards.Add(cardCreationResult.Card);
        }
        
        
        var resultCard = await CardSelectCmd.FromChooseACardScreen(
            new BlockingPlayerChoiceContext(), 
            cards,
            Owner
        );
        if (resultCard == null) return false;
            
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(resultCard, PileType.Deck));
        return true;
    }

    public override string OptionId => "LullMasterEmerald";
}