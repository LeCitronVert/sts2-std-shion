using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Yellow challenge (on each player): the first card you play each turn costs 1 more (first N cards with N stacks).
/// </summary>
public class YellowEmeraldChallenge : EmeraldChallengePower
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Yellow;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ShouldModifyCost(card, originalCost)) return false;
        modifiedCost = originalCost + 1;
        return true;
    }

    private bool ShouldModifyCost(CardModel card, decimal originalCost)
    {
        if (!CombatManager.Instance.IsInProgress) return false;
        if (card.Owner != Owner.Player || originalCost < 0 || card.EnergyCost.CostsX) return false;
        if (card.Pile?.Type is not (PileType.Hand or PileType.Play)) return false;

        var playedThisTurn = CombatManager.Instance.History.CardPlaysStarted
            .Count(e => e.Actor == Owner && e.CardPlay.IsFirstInSeries && e.HappenedThisTurn(CombatState));
        return playedThisTurn < Amount;
    }
}
