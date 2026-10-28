using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Dragon-Tiger Flash (Team-Up), on the Initiator: whenever you Patch Up, your Partner loses that much Damage Debt (times Amount).
/// Whenever your Partner ends their turn with Refuse, gain 10 Block (times Amount).
/// </summary>
public class DragonTigerFlash : TeamUpPower, IPatchUpListener
{
    public const int BlockPerRefuse = 10;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("BlockPerRefuse", BlockPerRefuse)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DamageDebt>(),
        HoverTipFactory.FromPower<Refuse>(),
    ];

    public async Task AfterPatchUp(PlayerChoiceContext choiceContext, Creature creature, int amount)
    {
        if (creature != Owner) return;

        foreach (var partner in LivingPartners.Where(p => FryMechanicsHelper.GetDamageDebt(p) > 0).ToList())
        {
            Flash();
            await FryMechanicsHelper.LoseDamageDebt(choiceContext, partner, amount * Amount, null);
        }
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner.IsDead) return;

        foreach (var partner in LivingPartners.Where(p => participants.Contains(p) && p.HasPower<Refuse>()).ToList())
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, BlockPerRefuse * Amount, ValueProp.Unpowered, null);
        }
    }
}
