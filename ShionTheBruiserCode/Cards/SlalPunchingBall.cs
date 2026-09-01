using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalPunchingBall(): SlalSlimyCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6M, ValueProp.Move),
        new PowerVar<WeakPower>(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
        
        ArgumentNullException.ThrowIfNull(play.Target);
        ArgumentNullException.ThrowIfNull(play.Target.Monster);
        if (!play.Target.Monster.IntendsToAttack)
            return;
        await PowerCmd.Apply<WeakPower>(
            choiceContext, 
            play.Target, 
            card.DynamicVars.Weak.BaseValue, 
            card.Owner.Creature, 
            card
        );
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars["WeakPower"].UpgradeValueBy(1);
    }
    
    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            return CombatState != null && CombatState.HittableEnemies.Any(e =>
            {
                MonsterModel? monster = e.Monster;
                return monster != null && monster.IntendsToAttack;
            });
        }
    }
}