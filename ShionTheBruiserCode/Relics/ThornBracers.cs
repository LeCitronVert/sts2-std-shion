using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Upgraded Thorn Bracelet. Whenever you lose HP, gain 2 Blood.
/// Whenever you play a Blood Bend card, gain 1 Blood after you consumed your Blood.
/// </summary>
public class ThornBracers : SeelsTheRecklessRelic, ISeelsListener
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Blood", 2M),
        new DynamicVar("BendBlood", 1M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
    ];

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature || result.UnblockedDamage <= 0) return;

        Flash();
        await SeelsMechanicsHelper.GainBlood(choiceContext, Owner.Creature, DynamicVars["Blood"].BaseValue);
    }

    public async Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source)
    {
        if (player != Owner || source is not CardModel) return;

        Flash();
        await SeelsMechanicsHelper.GainBlood(choiceContext, Owner.Creature, DynamicVars["BendBlood"].BaseValue);
    }
}
