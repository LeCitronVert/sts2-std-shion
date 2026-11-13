using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class CriticalAttackNext : CritContributorPower, ICritHitRateContributor
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Card.Owner.Creature == Owner && IsPoweredAttackCard(cardPlay.Card))
        {
            await PowerCmd.Decrement(this);
        }
    }

    // Not every attack has a "Damage" var (e.g. Dragon Claw only has CalculatedDamage) — never index it directly.
    private static bool IsPoweredAttackCard(CardModel card)
    {
        var vars = card.DynamicVars;
        if (vars.TryGetValue("Damage", out var dmg) && dmg is DamageVar damageVar)
            return damageVar.Props.IsPoweredAttack();
        if (vars.TryGetValue("CalculatedDamage", out var calc) && calc is CalculatedDamageVar calculatedVar)
            return calculatedVar.Props.IsPoweredAttack();
        return false;
    }

    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        if (sourceCard == null || sourceCard.Owner.Creature == Owner)
            return 1;
        
        return 0;
        
    }
}