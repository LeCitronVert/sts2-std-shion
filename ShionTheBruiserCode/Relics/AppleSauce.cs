using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Slimed cards are free to play. At the start of each combat, add 2 Slimed into your Draw Pile.
/// </summary>
public class AppleSauce : SlalTheEndlessRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
    ];

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner != Owner || card is not Slimed) return false;

        modifiedCost = 0M;
        return true;
    }

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.CombatState is null) return;

        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var slimed = Owner.Creature.CombatState.CreateCard<Slimed>(Owner);
            CardCmd.PreviewCardPileAdd(
                await CardPileCmd.AddGeneratedCardToCombat(slimed, PileType.Draw, Owner, CardPilePosition.Random)
            );
        }
    }
}
