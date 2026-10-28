using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Hyper Offense (Team-Up), on both players: your attacks on Vulnerable enemies gain Amount% Critical Hit chance.
/// Berserk: double this bonus.
/// </summary>
public class HyperOffense : ShionTheBruiserPower, ITargetedCritHitRateContributor
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

    public decimal GetCritHitRateBonus(CardModel sourceCard, IReadOnlyList<Creature> targets)
    {
        if (sourceCard.Owner.Creature != Owner || !targets.Any(t => t.HasPower<VulnerablePower>())) return 0m;

        var bonus = Amount / 100m;
        return ShionMechanicsHelper.IsBerserk(Owner) ? bonus * 2 : bonus;
    }
}
