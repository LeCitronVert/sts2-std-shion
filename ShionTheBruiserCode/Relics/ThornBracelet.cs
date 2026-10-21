using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Whenever you lose HP, gain 1 Blood.
/// </summary>
public class ThornBracelet : SeelsTheRecklessRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Blood", 1M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Blood>()];

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature || result.UnblockedDamage <= 0) return;

        Flash();
        await SeelsMechanicsHelper.GainBlood(choiceContext, Owner.Creature, DynamicVars["Blood"].BaseValue);
    }

    public override RelicModel GetUpgradeReplacement() => ModelDb.Relic<ThornBracers>();
}
