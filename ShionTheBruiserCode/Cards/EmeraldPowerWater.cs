using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Cyan Chaos Emerald (Emerald Power): play a copy of ALL the Power cards you played this combat.
/// Emerald Powers are always free, so its old cost (4, reduced on draw, -2 upgraded) is gone.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerWater() : EmeraldPowerCard(CardType.Skill, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Cyan;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("PowersAmount").WithMultiplier((card, _) => GetPowersPlayed(card).Count),
    ];

    private static List<CardModel> GetPowersPlayed(CardModel card)
    {
        if (card.CombatState is null) return [];
        return CombatManager.Instance.History.CardPlaysFinished
            .Where(e => e.CardPlay.Card.Owner == card.Owner && e.CardPlay.IsFirstInSeries && e.CardPlay.Card.Type == CardType.Power)
            .Select(e => e.CardPlay.Card)
            .ToList();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;

        // Listed first, so the copies played here aren't copied again
        foreach (var original in GetPowersPlayed(this))
        {
            if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead) break;

            var copy = CombatState.CloneCard(original);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
            await CardCmd.AutoPlay(choiceContext, copy, null);
        }
    }

    // Its upgrade used to lower its cost, which no longer exists
    protected override void OnUpgrade() //todo : upgrade not specified
    {
    }
}
