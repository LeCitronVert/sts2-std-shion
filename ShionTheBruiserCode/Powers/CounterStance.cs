using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Patches;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class CounterStance() : ShionTheBruiserPower, ICardAfflictionListener
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("StatsGain", StatsGain)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public const int StatsGain = 1;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier,
        CardModel? cardSource)
    {
        if (power.Type != PowerType.Debuff || power.Owner != Owner) 
        {
            return;
        }

        await GainBonus();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy) return;

        await PowerCmd.Decrement(this);
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Type != CardType.Status || card.Owner.Creature != Owner) return;
        
        await GainBonus();
    }

    public async Task GainBonus()
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            StatsGain,
            Owner,
            null
        );
        await PowerCmd.Apply<DexterityPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            StatsGain,
            Owner,
            null
        );
    }

    public async Task OnAfflictionApplied(AfflictionModel affliction, CardModel card)
    {
        if (card.Owner.Creature != Owner) return;
        
        await GainBonus();
    }
}