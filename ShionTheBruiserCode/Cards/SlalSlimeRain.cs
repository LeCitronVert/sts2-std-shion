using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalSlimeRain(): SlalSlimyCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        SlalCardKeywords.Slimy,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(((Func<CardModel, Creature, Decimal>) ((card, _) =>
        {
            return card.Owner.PlayerCombatState != null
                ? card.Owner.PlayerCombatState.AllCards.Count(c => c.Keywords.Contains(SlalCardKeywords.Slimy))
                : 0;
        }))!)
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState != null)
            
            await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
                .FromCard(card)
                .WithValueProp(ValueProp.Move)
                .TargetingAllOpponents(card.CombatState)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
                .Execute(choiceContext)
            ;
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}