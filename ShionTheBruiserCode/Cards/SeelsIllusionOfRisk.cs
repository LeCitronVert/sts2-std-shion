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
/// Double your Vulnerable. For the next 2 (3) turns, if you are Vulnerable, you take 75% reduced damage from Vulnerable enemies.
/// </summary>
public class SeelsIllusionOfRisk() : SeelsTheRecklessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<IllusionOfRisk>(2M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        var vulnerable = SeelsMechanicsHelper.GetVulnerable(creature);
        if (vulnerable > 0)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, creature, vulnerable, creature, this);
        }
        await PowerCmd.Apply<IllusionOfRisk>(choiceContext, creature, DynamicVars["IllusionOfRisk"].BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["IllusionOfRisk"].UpgradeValueBy(1M);
    }
}
