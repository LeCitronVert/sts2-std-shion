using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionPsychoCut() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<EnergyDamageUp>(3),
        new DamageVar(8, ValueProp.Move),
        new CriticalDrawVar(33)
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithEnergyBlast(card, "vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
        
        await PowerCmd.Apply<EnergyDamageUp>(
            choiceContext,
            card.Owner.Creature,
            DynamicVars["EnergyDamageUp"].BaseValue,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EnergyDamageUp"].UpgradeValueBy(1);
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["CriticalDraw"].UpgradeValueBy(17);
    }
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
    }
}