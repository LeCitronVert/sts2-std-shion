using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Strife;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Red Chaos Emerald Burst (Emerald Power, 2 (1)-cost Power): Stun ALL enemies.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerBullet() : EmeraldPowerCard(2, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Red;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Stun)];

    protected override async Task OnEmeraldPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is not { } combatState) return;
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            if (CombatManager.Instance.IsOverOrEnding) return;
            if (enemy.IsAlive) await CreatureCmd.Stun(enemy);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
