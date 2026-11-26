using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Yellow Chaos Emerald (Emerald Power): this turn, all enemies are Slowed (base game Slow), starting at 50% (100%).
/// Stacks with any Slow they already have (one shared %).
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerSlow() : EmeraldPowerCard(CardType.Skill, TargetType.AllEnemies)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Yellow;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("SlowValue", 50)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SlowPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;
        foreach (var enemy in CombatState.Enemies.Where(e => e.IsAlive).ToList())
        {
            await EmeraldSlow.Apply(choiceContext, enemy, DynamicVars["SlowValue"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SlowValue"].UpgradeValueBy(50);
    }
}
