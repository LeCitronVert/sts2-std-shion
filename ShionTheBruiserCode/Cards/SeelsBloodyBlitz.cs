using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Defect): you and your partner draw 3 cards and reduce their cost by 1 this turn,
/// then discard those of them whose cost is not 0.
/// </summary>
public class SeelsBloodyBlitz() : SeelsTeamUpCard(2,
    CardType.Skill,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Defect>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3),
        new DynamicVar("CostReduction", 1),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        foreach (var player in new[] { initiator, partner })
        {
            var drawn = (await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, player)).ToList();
            foreach (var card in drawn.Where(c => !c.EnergyCost.CostsX))
            {
                card.EnergyCost.AddThisTurn(-DynamicVars["CostReduction"].IntValue, reduceOnly: true);
            }

            var toDiscard = drawn.Where(c => c.Pile?.Type == PileType.Hand
                && (c.EnergyCost.CostsX || c.EnergyCost.GetWithModifiers(CostModifiers.All) != 0)).ToList();
            if (toDiscard.Count > 0)
            {
                await CardCmd.Discard(choiceContext, toDiscard);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
