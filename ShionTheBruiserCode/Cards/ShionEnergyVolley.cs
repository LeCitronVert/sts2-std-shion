using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyVolley() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public const int FirstGalvanisedThreshold = 10;
    public const int SecondGalvanisedThreshold = 20;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1, ValueProp.Move),
        new CalculationBaseVar(3),
        new CalculationExtraVar(2),
        new CalculatedVar("HitCount").WithMultiplier((model, _) => GetGalvanisedTiers(model.Owner.Creature)),
        new DynamicVar("CostIncrease", 1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];
    
    protected override bool ShouldGlowGoldInternal => GetGalvanisedTiers(this.Owner.Creature) > 0;

    // Number of Galvanised bonuses currently met (0, 1 or 2)
    private static int GetGalvanisedTiers(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature)
    {
        return (ShionMechanicsHelper.IsGalvanised(creature, FirstGalvanisedThreshold) ? 1 : 0)
             + (ShionMechanicsHelper.IsGalvanised(creature, SecondGalvanisedThreshold) ? 1 : 0);
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this) return false;
        
        var tiers = GetGalvanisedTiers(Owner.Creature);
        if (tiers <= 0) return false;
        
        modifiedCost = originalCost + tiers * DynamicVars["CostIncrease"].IntValue;
        return true;
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.TriggerAnim(
            card.Owner.Creature,
            Character.ShionTheBruiser.GetEnergyCastAnimIfApplicable(card.Owner.Character),
            Character.ShionTheBruiser.GetEnergyCastDelayIfApplicable(card.Owner.Character)
        );
        // Also covers the cost increase, which is a Modify hook
        ShionMechanicsHelper.TriggerGalvanised(card.Owner.Creature, FirstGalvanisedThreshold);
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .WithHitCount((int) ((CalculatedVar) card.DynamicVars["HitCount"]).Calculate(card.Owner.Creature))
            .FromCard(card)
            .Targeting(play.Target)
            .WithEnergyBlast(card, "vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}
