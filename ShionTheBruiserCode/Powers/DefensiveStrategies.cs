using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class DefensiveStrategies : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Defensive),
        HoverTipFactory.FromPower<VigorPower>(),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
    ];

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (
            cardPlay.Card.Owner.Creature != Owner
            || !cardPlay.Card.Keywords.Contains(ShionCardKeywords.Defensive)
        ) return Task.CompletedTask;

        var power = this;
        PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), power.Owner, Amount, power.Owner, null!);
        ShionMechanicsHelper.EnsureCreatureHasCritTracker(power.Owner);
        PowerCmd.Apply<CritRateUp>(new ThrowingPlayerChoiceContext(), power.Owner, Amount, power.Owner, null!);
        
        return Task.CompletedTask;
    }
}