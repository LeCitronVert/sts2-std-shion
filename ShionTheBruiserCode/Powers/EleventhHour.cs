using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class EleventhHour() : ShionTheBruiserPower, IOnCriticalHitListener, IOnCriticalFailListener
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public const int StrengthBonus = 5;
    public const int StatsMalus = 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Bonus", StrengthBonus),
        new DynamicVar("Malus", StatsMalus)
    ];
    
    public Task OnCriticalHit(Creature attacker, Creature target, DamageResult damageResult, ValueProp props,
        CardModel? sourceCard, PlayerChoiceContext? choiceContext)
    {
        if (attacker == Owner && sourceCard is { Type: CardType.Attack } && props.IsPoweredAttack())
        {
            PowerCmd.Apply<StrengthPower>(
                choiceContext ?? new ThrowingPlayerChoiceContext(), 
                Owner, 
                StrengthBonus, 
                Owner, 
                null
            );
            
            PowerCmd.Decrement(this);
        }

        return Task.CompletedTask;
    }

    public Task OnCriticalFail(Creature attacker, Creature target, DamageResult damageResult, ValueProp props,
        CardModel? sourceCard, PlayerChoiceContext? choiceContext)
    {
        if (attacker == Owner && sourceCard is { Type: CardType.Attack } && props.IsPoweredAttack())
        {
            PowerCmd.Apply<WeakPower>(
                choiceContext ?? new ThrowingPlayerChoiceContext(), 
                Owner, 
                StatsMalus, 
                Owner, 
                null
            );
            
            PowerCmd.Apply<VulnerablePower>(
                choiceContext ?? new ThrowingPlayerChoiceContext(), 
                Owner, 
                StatsMalus, 
                Owner, 
                null
            );
            
            PowerCmd.Apply<DebilitatePower>(
                choiceContext ?? new ThrowingPlayerChoiceContext(), 
                Owner, 
                StatsMalus, 
                Owner, 
                null
            );

            PowerCmd.Decrement(this);
            PlayerCmd.EndTurn(sourceCard.Owner, false);
        }

        return Task.CompletedTask;
    }
}