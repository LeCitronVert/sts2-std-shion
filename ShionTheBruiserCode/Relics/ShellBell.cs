using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class ShellBell : CritContributorRelic, IOnCriticalHitListener, ICritHitRateContributor
{
    public override RelicRarity Rarity =>
        RelicRarity.Common;

    public const int HealAmount = 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CritDisplayTooltipVar(),
        new DynamicVar("CritHitRate", 5),
        new HealVar(HealAmount)
    ];

    public async Task OnCriticalHit(Creature attacker, Creature target, DamageResult damageResult, ValueProp props,
        CardModel? sourceCard, PlayerChoiceContext? choiceContext)
    {
        if (sourceCard == null || sourceCard.Owner != Owner) return;

        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);
    }

    public decimal GetCritHitRateBonus(CardModel? sourceCard)
    {
        if (sourceCard != null && sourceCard.Owner.Creature != Owner.Creature) return 0;
        
        var bonus = DynamicVars["CritHitRate"].BaseValue / 100;
        return bonus;
    }
}