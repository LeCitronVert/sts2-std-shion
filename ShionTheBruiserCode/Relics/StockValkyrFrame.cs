using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// When you deal unblocked Attack damage, the Valkyrs attack for 25% of that damage (rounded down).
/// </summary>
public class StockValkyrFrame : FryTheCommanderRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Percent", 25M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(FryCardKeywords.Valkyrs)];

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner.Creature || !props.IsPoweredAttack() || FryMechanicsHelper.IsValkyrDamage) return;
        if (result.UnblockedDamage <= 0 || target.IsDead) return;

        var amount = Math.Floor(result.UnblockedDamage * DynamicVars["Percent"].BaseValue / 100M);
        if (amount <= 0) return;

        Flash();
        await FryMechanicsHelper.ValkyrsDamage(choiceContext, [target], amount);
    }
}
