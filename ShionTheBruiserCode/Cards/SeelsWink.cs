using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 4 (6) Block. Gamble 1. Apply Weak equal to this card's energy cost to ALL enemies.
/// </summary>
public class SeelsWink() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.AllEnemies)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Gamble];

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(4M, ValueProp.Move),
        new DynamicVar("Gamble", 1M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("WeakAmount").WithMultiplier((card, _) => card.EnergyCost.GetResolved()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Gamble),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic),
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // Read the cost before Gambling: it doesn't change, but this keeps it tied to the play itself.
        var weak = EnergyCost.GetResolved();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await SeelsMechanicsHelper.Gamble(choiceContext, Owner, DynamicVars["Gamble"].IntValue, this);

        if (weak > 0 && CombatState is not null)
        {
            var enemies = CombatState.HittableEnemies.ToList();
            if (enemies.Count > 0)
            {
                await PowerCmd.Apply<WeakPower>(choiceContext, enemies, weak, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2M);
    }
}
