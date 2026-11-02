using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Generic Team Blast. ALL players gain 3 (5) Energy and 3 (5) Stars, and draw 3 (5) cards.
/// </summary>
[Pool(typeof(ColorlessCardPool))]
public class Cacophony() : TeamBlastCard(CardType.Skill, TargetType.AllAllies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(3),
        new StarsVar(3),
        new CardsVar(3),
    ];

    protected override async Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (var player in LivingPlayers)
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);
            await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, player);
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, player);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(2);
        DynamicVars.Stars.UpgradeValueBy(2);
        DynamicVars.Cards.UpgradeValueBy(2);
    }
}
