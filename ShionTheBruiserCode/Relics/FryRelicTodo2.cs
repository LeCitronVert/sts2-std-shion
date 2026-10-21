using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Upgraded Fry starter relic. Whenever you end your turn, gain Stars equal to your remaining energy +1.
/// Start each combat with 3 Stars.
/// </summary>
public class FryRelicTodo2 : FryRelicTodo1
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BonusStars", 1M),
        new StarsVar(3),
    ];

    protected override int BonusStars => DynamicVars["BonusStars"].IntValue;

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
    }

    public override RelicModel? GetUpgradeReplacement() => null;
}
