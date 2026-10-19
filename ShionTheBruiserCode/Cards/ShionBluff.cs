using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionBluff() : ShionBerserkCard(0,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VigorPower>(3),
        new DynamicVar("EnemyVigor", 3),
        new PowerVar<StrengthPower>(1M)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionBluff source = this;
        await CreatureCmd.TriggerAnim(source.Owner.Creature, "Cast", source.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<VigorPower>(choiceContext, source.Owner.Creature, source.DynamicVars["VigorPower"].BaseValue, source.Owner.Creature, source);
        
        if (ShionMechanicsHelper.IsBerserk(source.Owner.Creature))
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, source.Owner.Creature, source.DynamicVars["StrengthPower"].BaseValue, source.Owner.Creature, source);
            return;
        }
        
        if (source.CombatState == null) return;
        var enemy = source.Owner.RunState.Rng.CombatTargets.NextItem(source.CombatState.HittableEnemies);
        if (enemy == null) return;
        
        await PowerCmd.Apply<VigorPower>(choiceContext, enemy, source.DynamicVars["EnemyVigor"].BaseValue, source.Owner.Creature, source);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorPower"].UpgradeValueBy(2);
        DynamicVars["StrengthPower"].UpgradeValueBy(1M);
    }
}
