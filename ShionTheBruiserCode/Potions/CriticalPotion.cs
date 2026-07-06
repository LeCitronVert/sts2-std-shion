using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

[Pool(typeof(SharedPotionPool))]
public class CriticalPotion: ShionTheBruiserPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    public const int CritHitRate = 20;
    public const int CritAttackRate = 10;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("CritHitRate", CritHitRate),
        new DynamicVar("CritAttackRate", CritAttackRate),
        new CritDisplayTooltipVar(),
    ];
    
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var potion = this;
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("fd2155"));
        
        if (!target.HasPower<ShionCritTrackerPower>())
        {
            await PowerCmd.Apply<ShionCritTrackerPower>(
                choiceContext,
                target,
                amount: 1m,
                applier: potion.Owner.Creature,
                cardSource: null);
        }
        await PowerCmd.Apply<CritRateUp>(
            choiceContext,
            target,
            CritHitRate,
            potion.Owner.Creature,
            null
        );
        await PowerCmd.Apply<CritAttackUp>(
            choiceContext,
            target,
            CritAttackRate,
            potion.Owner.Creature,
            null
        );
    }
}