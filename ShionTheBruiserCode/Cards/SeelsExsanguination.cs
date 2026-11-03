using MegaCrit.Sts2.Core.Combat;
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
/// Blood Bend X. Deal X damage. Increase this damage by X times the number of Attacks you played this turn
/// (this one included), times the energy cost of this card. Exhaust. (Upgrade: lose Exhaust.)
/// </summary>
public class SeelsExsanguination() : SeelsTheRecklessCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            GetDamage(card, SeelsMechanicsHelper.PreviewBloodBendX(card))),
        new CalculatedVar("AttacksAmount").WithMultiplier((card, _) => CountAttacksPlayedThisTurn(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
    ];

    /// <summary>
    /// Attacks played this turn, this card included (it hasn't finished being played yet, so it's added by hand).
    /// </summary>
    private static int CountAttacksPlayedThisTurn(CardModel card)
    {
        if (card.CombatState is null) return 0;
        var finished = CombatManager.Instance.History.CardPlaysFinished
            .Count(e => e.HappenedThisTurn(card.CombatState)
                        && e.CardPlay.Card.Owner == card.Owner
                        && e.CardPlay.IsFirstInSeries
                        && e.CardPlay.Card.Type == CardType.Attack);
        return finished + 1;
    }

    private static int GetDamage(CardModel card, int x) =>
        x + x * CountAttacksPlayedThisTurn(card) * card.EnergyCost.GetResolved();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        // Read before Blood Bending, so nothing triggered by it can change the count.
        var attacks = CountAttacksPlayedThisTurn(this);
        var cost = EnergyCost.GetResolved();
        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);
        await DamageCmd.Attack(x + x * attacks * cost)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
