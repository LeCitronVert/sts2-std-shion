using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Fellchion (Team-Up), on the Initiator: whenever you lose HP, your Partner Forges that much (times Amount).
/// When your Partner's Sovereign Blade deals unblocked damage, heal that much (times Amount).
/// </summary>
public class Fellchion : TeamUpPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SovereignBlade>()];

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner || delta >= 0) return;

        foreach (var partner in LivingPartners.ToList())
        {
            if (partner.Player is not { } player) continue;

            Flash();
            await ForgeCmd.Forge(-delta * Amount, player, this);
        }
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (cardSource is not SovereignBlade || dealer == null || !Partners.Contains(dealer)) return;
        if (result.UnblockedDamage <= 0 || Owner.IsDead) return;

        Flash();
        await CreatureCmd.Heal(Owner, result.UnblockedDamage * Amount);
    }
}
