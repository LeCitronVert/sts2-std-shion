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
/// Add 1 (2) Slash into your Hand. Blood Bend X. The enemy loses X Strength this turn. You gain X Strength this turn. Exhaust.
/// </summary>
public class SeelsHack() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("XAmount").WithMultiplier((card, _) => SeelsMechanicsHelper.PreviewBloodBendX(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SeelsSlash>(),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await SeelsMechanicsHelper.AddSlashes(Owner, DynamicVars.Cards.IntValue, PileType.Hand, false);

        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);
        if (x <= 0) return;

        if (play.Target.IsAlive)
        {
            await PowerCmd.Apply<HackStrengthDown>(choiceContext, play.Target, x, Owner.Creature, this);
        }
        await PowerCmd.Apply<HackStrength>(choiceContext, Owner.Creature, x, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1M);
    }
}
