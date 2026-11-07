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
/// Blood Bend X. Gain 4 Block. Apply X (X+1) Vulnerable. Exhaust.
/// </summary>
public class SeelsBloodCage() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(4M, ValueProp.Move),
        new DynamicVar("Bonus", 0M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("VulnerableAmount").WithMultiplier((card, _) =>
            SeelsMechanicsHelper.PreviewBloodBendX(card) + card.DynamicVars["Bonus"].IntValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        var vulnerable = x + DynamicVars["Bonus"].IntValue;
        if (vulnerable > 0 && play.Target.IsAlive)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, vulnerable, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(1M);
    }
}
