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
/// Apply 2 (3) Vulnerable. Add 1 Perfect Fit Slash (Slash+) into your Discard Pile for each Vulnerable on the enemy.
/// Repeat if you are Vulnerable.
/// </summary>
public class SeelsThreateningPromise() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(2M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromCard<SeelsSlash>(IsUpgraded),
        ..HoverTipFactory.FromEnchantment<PerfectFit>(),
    ];

    protected override bool ShouldGlowGoldInternal => Owner.Creature.HasPower<VulnerablePower>();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var repeats = Owner.Creature.HasPower<VulnerablePower>() ? 2 : 1;

        for (var i = 0; i < repeats && play.Target.IsAlive; i++)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);

            var amount = SeelsMechanicsHelper.GetVulnerable(play.Target);
            var slashes = Enumerable.Range(0, amount).Select(_ =>
            {
                var slash = SeelsMechanicsHelper.CreateSlash(Owner, IsUpgraded);
                SeelsMechanicsHelper.Enchant<PerfectFit>(slash, 1);
                return slash;
            }).ToList();
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(slashes, PileType.Discard, Owner));
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1M);
    }
}
