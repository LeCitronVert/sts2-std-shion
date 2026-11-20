using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// The first time you reach 10 Stars each combat, gain 10 more.
/// </summary>
public class InhibitorRing : FryTheCommanderRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Threshold", 10M),
        new StarsVar(10),
    ];

    private bool _usedThisCombat;

    public override Task BeforeCombatStart()
    {
        _usedThisCombat = false;
        Status = RelicStatus.Active;
        return Task.CompletedTask;
    }

    public override async Task AfterStarsGained(int amount, Player gainer)
    {
        if (_usedThisCombat || gainer != Owner || amount <= 0) return;
        if ((Owner.PlayerCombatState?.Stars ?? 0) < DynamicVars["Threshold"].IntValue) return;

        _usedThisCombat = true;
        Status = RelicStatus.Normal;
        Flash();
        await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
    }
}
