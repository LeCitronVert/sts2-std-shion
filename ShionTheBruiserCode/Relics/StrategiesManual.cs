using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Whenever you Request, gain 1 Strength this turn (once per Request card played, even with several counters).
/// Called from <see cref="FryMechanicsHelper.Request"/>.
/// </summary>
public class StrategiesManual : FryTheCommanderRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FryCardKeywords.Request),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    public async Task OnRequest(PlayerChoiceContext choiceContext)
    {
        Flash();
        await PowerCmd.Apply<StrategiesManualStrength>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, null);
    }
}
