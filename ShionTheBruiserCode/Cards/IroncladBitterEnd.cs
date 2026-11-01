using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Ironclad Team Blast. Apply 1 (2) Vulnerable to ALL enemies.
/// For each enemy, ALL players put 1 (2) random Attack from their Draw Pile into their Hand,
/// then the highest cost Attack in their Hand costs 0 this turn.
/// </summary>
[Pool(typeof(IroncladCardPool))]
public class IroncladBitterEnd() : TeamBlastCard(CardType.Skill, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VulnerablePower>(1),
        new DynamicVar("Attacks", 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;

        var enemies = CombatState.HittableEnemies.ToList();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemies, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);

        var rng = Owner.RunState.Rng.CombatCardSelection;
        foreach (var _ in enemies)
        {
            foreach (var player in LivingPlayers)
            {
                for (var i = 0; i < DynamicVars["Attacks"].IntValue; i++)
                {
                    var attacks = PileType.Draw.GetPile(player).Cards.Where(c => c.Type == CardType.Attack).ToList();
                    if (attacks.Count == 0 || PileType.Hand.GetPile(player).Cards.Count >= CardPile.MaxCardsInHand) break;
                    await CardPileCmd.Add(rng.NextItem(attacks)!, PileType.Hand);
                }

                var highest = PileType.Hand.GetPile(player).Cards
                    .Where(c => c.Type == CardType.Attack && !c.EnergyCost.CostsX)
                    .Where(c => c.EnergyCost.GetWithModifiers(CostModifiers.All) > 0)
                    .OrderByDescending(c => c.EnergyCost.GetWithModifiers(CostModifiers.All))
                    .FirstOrDefault();
                highest?.EnergyCost.SetThisTurn(0);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1);
        DynamicVars["Attacks"].UpgradeValueBy(1);
    }
}
