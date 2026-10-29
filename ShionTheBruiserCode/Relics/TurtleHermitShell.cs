using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Shion's Neow relic (ShionAncientRelicPatches). Replace your starter Deck with one focused on Energy moves.
/// Berserk Threshold is reduced by 20% (ShionMechanicsHelper.GetBerserkThreshold).
/// </summary>
[Pool(typeof(EventRelicPool))]
public class TurtleHermitShell : ShionTheBruiserRelic
{
    public const int BerserkThresholdReduction = 20;

    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BerserkThresholdReduction", BerserkThresholdReduction),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
        HoverTipFactory.FromCard<ShionEnergyStrike>(),
        HoverTipFactory.FromCard<ShionEnergyShield>(),
        HoverTipFactory.FromCard<ShionEnergyBind>(),
        HoverTipFactory.FromCard<ShionEnergyBurst>(),
    ];

    private static IEnumerable<CardModel> NewStarterDeck =>
    [
        ModelDb.Card<ShionEnergyStrike>(),
        ModelDb.Card<ShionEnergyStrike>(),
        ModelDb.Card<ShionEnergyStrike>(),
        ModelDb.Card<ShionEnergyStrike>(),
        ModelDb.Card<ShionEnergyShield>(),
        ModelDb.Card<ShionEnergyShield>(),
        ModelDb.Card<ShionEnergyShield>(),
        ModelDb.Card<ShionEnergyShield>(),
        ModelDb.Card<ShionEnergyBind>(),
        ModelDb.Card<ShionEnergyBurst>(),
    ];

    public override bool IsAllowedAtNeow(Player player)
    {
        return player.Character is Character.ShionTheBruiser;
    }

    public override async Task AfterObtained()
    {
        // The starter Deck: every card of the character's starting deck still in the Deck
        var starterIds = Owner.Character.StartingDeck.Select(c => c.Id).ToHashSet();
        var starters = Owner.Deck.Cards.Where(c => starterIds.Contains(c.Id)).ToList();
        if (starters.Count > 0)
        {
            await CardPileCmd.RemoveFromDeck(starters, showPreview: false);
        }

        List<CardPileAddResult> results = [];
        foreach (var canonical in NewStarterDeck)
        {
            results.Add(await CardPileCmd.Add(Owner.RunState.CreateCard(canonical, Owner), PileType.Deck));
        }
        CardCmd.PreviewCardPileAdd(results, 2f);
    }
}
