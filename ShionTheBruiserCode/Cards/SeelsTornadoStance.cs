using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Add Replay to ALL your Slashes (and Upgrade them). Gain 1 Vulnerable for each of your Slashes. Doesn't include the Exhausted ones.
/// </summary>
public class SeelsTornadoStance() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SeelsSlash>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var slashes = SeelsMechanicsHelper.GetAllCombatCards(Owner)
            .Where(c => SeelsMechanicsHelper.IsSlash(c) && c.Pile?.Type != PileType.Exhaust)
            .ToList();

        foreach (var slash in slashes)
        {
            slash.BaseReplayCount += 1;
            if (IsUpgraded && slash.IsUpgradable) CardCmd.Upgrade(slash);
            SeelsMechanicsHelper.CountModification(slash);
        }

        if (slashes.Count > 0)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, slashes.Count, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
