using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// The first time you gain Damage Debt each combat, Refuse 1.
/// </summary>
public class FryRelicTodo3 : FryTheCommanderRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<Refuse>(1M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DamageDebt>(),
        HoverTipFactory.FromPower<Refuse>(),
    ];

    private bool _usedThisCombat;

    public override Task BeforeCombatStart()
    {
        _usedThisCombat = false;
        Status = RelicStatus.Active;
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_usedThisCombat || power is not DamageDebt || power.Owner != Owner.Creature || amount <= 0) return;

        _usedThisCombat = true;
        Status = RelicStatus.Normal;
        Flash();
        await FryMechanicsHelper.Refuse(choiceContext, Owner.Creature, DynamicVars["Refuse"].BaseValue, null);
    }
}
