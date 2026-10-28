using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Seels): randomize the cost of ALL cards in you and your partner's hands (0 to 3, this turn).
/// For each card whose cost is less or equal to its original cost, its owner gains 1 Strength and Dexterity.
/// </summary>
public class ShionBiggestGamble() : ShionTeamUpCard(0,
    CardType.Skill,
    TargetType.Self)
{
    public const int MaxRandomCost = 3;

    public override CharacterModel PartnerCharacter => ModelDb.Character<SeelsTheReckless>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxRandomCost", MaxRandomCost),
        new PowerVar<StrengthPower>(1),
        new PowerVar<DexterityPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        foreach (var player in new[] { initiator, partner })
        {
            foreach (var card in PileType.Hand.GetPile(player).Cards.Where(c => !c.EnergyCost.CostsX).ToList())
            {
                var originalCost = card.EnergyCost.GetWithModifiers(CostModifiers.None);
                if (originalCost < 0) continue;

                var cost = Owner.RunState.Rng.CombatEnergyCosts.NextInt(0, MaxRandomCost + 1);
                card.EnergyCost.SetThisTurn(cost);
                if (cost > originalCost) continue;

                var creature = player.Creature;
                await PowerCmd.Apply<StrengthPower>(choiceContext, creature, DynamicVars.Strength.BaseValue, creature, this);
                await PowerCmd.Apply<DexterityPower>(choiceContext, creature, DynamicVars.Dexterity.BaseValue, creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
