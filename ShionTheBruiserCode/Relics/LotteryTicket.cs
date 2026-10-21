using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// At the start of each combat, choose up to 3 cards in your Draw Pile, add Erratic to them and randomize their energy cost.
/// Gain 1 Vulnerable for each card modified.
/// </summary>
public class LotteryTicket : SeelsTheRecklessRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner || Owner.PlayerCombatState?.TurnNumber > 1) return;

        var cards = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Draw.GetPile(Owner),
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, DynamicVars.Cards.IntValue)
        )).ToList();
        if (cards.Count == 0) return;

        Flash();
        foreach (var card in cards)
        {
            SeelsMechanicsHelper.AddErratic(card);
            await SeelsMechanicsHelper.RandomizeErraticCost(choiceContext, card);
        }
        await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, cards.Count, Owner.Creature, null);
    }
}
