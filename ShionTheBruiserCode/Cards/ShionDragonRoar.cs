using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionDragonRoar() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(1),
        // Damage scales with missing hp, up until the maximum which is stored in ExtraDamageVar
        new CalculatedVar("DebuffAmount").WithMultiplier((model, creature) => 
            model.DynamicVars.CalculationBase.BaseValue * ShionMechanicsHelper.GetDragonUniqueBuffCount(model.Owner.Creature)
        ),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var cardSource = this;
        await CreatureCmd.TriggerAnim(cardSource.Owner.Creature, "Cast", cardSource.Owner.Character.CastAnimDelay);
        foreach (Creature hittableEnemy in (IEnumerable<Creature>) cardSource.CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<DragonRoar>(choiceContext, hittableEnemy, cardSource.DynamicVars["StrengthLoss"].BaseValue, cardSource.Owner.Creature, (CardModel) cardSource);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(1);
    }
}