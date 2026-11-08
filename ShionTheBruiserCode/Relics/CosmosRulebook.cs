using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (not cursed, solo and multiplayer). Upon pickup, add 1 Cosmo's Plea to your Deck.
/// </summary>
public class CosmosRulebook : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<CosmoPlea>();

    public override async Task AfterObtained()
    {
        List<CardPileAddResult> results = [];
        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var card = Owner.RunState.CreateCard<CosmoPlea>(Owner);
            results.Add(await CardPileCmd.Add(card, PileType.Deck));
        }
        CardCmd.PreviewCardPileAdd(results, 2f);
    }
}
