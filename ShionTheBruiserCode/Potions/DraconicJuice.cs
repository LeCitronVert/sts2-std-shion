using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Potions;

public class DraconicJuice: ShionTheBruiserPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    public const int BuffAmount = 1;
    
    public override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<CritAttackUp>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<RegenPower>()
    ];
    
    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var potion = this;
        PotionModel.AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("fd2155"));
        
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            target,
            BuffAmount,
            potion.Owner.Creature,
            null
        );
        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            target,
            BuffAmount,
            potion.Owner.Creature,
            null
        );
        await PowerCmd.Apply<CritAttackUp>(
            choiceContext,
            target,
            BuffAmount,
            potion.Owner.Creature,
            null
        );
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            target,
            BuffAmount,
            potion.Owner.Creature,
            null
        );
        await PowerCmd.Apply<RegenPower>(
            choiceContext,
            target,
            BuffAmount,
            potion.Owner.Creature,
            null
        );
    }
}