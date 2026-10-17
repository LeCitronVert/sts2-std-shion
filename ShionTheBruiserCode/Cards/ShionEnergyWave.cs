using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyWave() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(3),
        new ExtraDamageVar(2),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(((model, creature) => model.Owner.Creature.GetPowerAmount<VigorPower>() > 0 ? 1 : 0)),
        new PowerVar<VulnerablePower>(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var alsoApplyVulnerable = card.Owner.Creature.GetPowerAmount<VigorPower>() > 0;
        
        if (card.CombatState != null)
            await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
                .FromCard(card)
                .WithValueProp(ValueProp.Move)
                .TargetingAllOpponents(card.CombatState)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
                .Execute(choiceContext)
            ;

        if (alsoApplyVulnerable)
        {
            foreach (Creature hittableEnemy in card.CombatState?.HittableEnemies ?? [])
            {
                // idk why i can't just use the dynamic var here, i prolly fucked up something
                await PowerCmd.Apply<VulnerablePower>(choiceContext, hittableEnemy, card.DynamicVars["VulnerablePower"].IntValue, card.Owner.Creature, card);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2);
        DynamicVars.ExtraDamage.UpgradeValueBy(2);
        DynamicVars["VulnerablePower"].UpgradeValueBy(1);
    }
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.HasVigor(this.Owner.Creature);
}