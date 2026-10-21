using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Blood Bend X. Deal X damage. Deal 2 (3) additional damage for ALL your cards containing "Blood".
/// </summary>
public class SeelsBloodyExecution() : SeelsTheRecklessCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("PerBloodCard", 2M),
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => CountBloodCards(card) * card.DynamicVars["PerBloodCard"].BaseValue),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
    ];

    /// <summary>Cards containing "Blood": the ones tagged Blood, plus Bloodletting and Blood Wall.</summary>
    public static bool ContainsBlood(CardModel card)
    {
        return card.Tags.Contains(SeelsCardTags.Blood) || card is Bloodletting or BloodWall;
    }

    private static int CountBloodCards(CardModel card)
    {
        return SeelsMechanicsHelper.GetAllCombatCards(card.Owner).Count(ContainsBlood);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);
        var damage = x + CountBloodCards(this) * DynamicVars["PerBloodCard"].BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PerBloodCard"].UpgradeValueBy(1M);
    }
}
