using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class BreakMeIfYouCan : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.None;

    public const int GuardAmount = 1;
    public const int VigorAmount = 3;
    public const int BlockAmount = 5;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("GuardAmount", GuardAmount),
        new DynamicVar("VigorAmount", VigorAmount),
        new DynamicVar("BlockAmount", BlockAmount),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Guard>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        
        Flash();
        await GainBonuses(choiceContext, Owner);
    }

    public static async Task GainBonuses(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Entities.Creatures.Creature creature)
    {
        await PowerCmd.Apply<Guard>(choiceContext, creature, GuardAmount, creature, null);
        await PowerCmd.Apply<VigorPower>(choiceContext, creature, VigorAmount, creature, null);
        await CreatureCmd.GainBlock(creature, new BlockVar(BlockAmount, ValueProp.Unpowered), null!);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Attack) return;

        await PowerCmd.Remove(this);
    }
}
