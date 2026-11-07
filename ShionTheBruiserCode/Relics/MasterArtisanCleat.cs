using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// At the start of combat, add 1 random Rare Colourless card to your Hand. It is free to play this combat.
/// </summary>
public class MasterArtisanCleat() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner || Owner.PlayerCombatState?.TurnNumber != 1) return;

        var options = ModelDb.CardPool<ColorlessCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Rarity == CardRarity.Rare && c.CanBeGeneratedInCombat);

        var cards = CardFactory.GetDistinctForCombat(
            Owner,
            options,
            DynamicVars.Cards.IntValue,
            Owner.RunState.Rng.CombatCardGeneration
        ).ToList();
        if (cards.Count == 0) return;

        Flash();
        foreach (var card in cards.Where(c => !c.EnergyCost.CostsX))
        {
            card.SetToFreeThisCombat();
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner);
    }
}
