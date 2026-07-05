using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class SMonitor() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<BufferPower>()
    ];

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        var relic = this;
        if (!participants.Contains<Creature>(relic.Owner.Creature) || relic.Owner.PlayerCombatState?.TurnNumber != 1)
            return;
        await PowerCmd.Apply<BufferPower>(
            new ThrowingPlayerChoiceContext(),
            relic.Owner.Creature,
            DynamicVars["SpiritBreak"].BaseValue,
            relic.Owner.Creature,
            null
        );
    }
}