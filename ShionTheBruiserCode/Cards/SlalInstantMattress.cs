using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Another player gains 4 Block and ignores Vulnerable until your next turn. Gain 4 Block.
/// Upgraded: ALL players gain 6 Block and ignore Vulnerable until your next turn.
/// </summary>
public class SlalInstantMattress() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override TargetType TargetType => IsUpgraded ? TargetType.AllAllies : TargetType.AnyAlly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4M, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<IgnoreVulnerable>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;
        var owner = card.Owner.Creature;

        List<Creature> receivers;
        if (IsUpgraded)
        {
            receivers = card.CombatState.Allies.Where(c => c.IsAlive && c.Player != null).ToList();
        }
        else
        {
            ArgumentNullException.ThrowIfNull(play.Target);
            receivers = [play.Target];
        }

        foreach (var receiver in receivers)
        {
            await CreatureCmd.GainBlock(receiver, card.DynamicVars.Block, play);
            await PowerCmd.Apply<IgnoreVulnerable>(choiceContext, receiver, 1, owner, card);
        }

        // Not upgraded: you only gain the Block
        if (!IsUpgraded)
        {
            await CreatureCmd.GainBlock(owner, card.DynamicVars.Block, play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
