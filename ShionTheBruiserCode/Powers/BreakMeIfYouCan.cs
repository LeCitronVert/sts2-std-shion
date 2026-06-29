using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class BreakMeIfYouCan : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public const int RegenAmount = 2;
    public const int VigorAmount = 7;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RegenAmount", RegenAmount),
        new DynamicVar("VigorAmount", VigorAmount)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<RegenPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner, RegenAmount, Owner, null);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, VigorAmount, Owner, null);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Attack) return;

        await PowerCmd.Remove(this);
    }
}