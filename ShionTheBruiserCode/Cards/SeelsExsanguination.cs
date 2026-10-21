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
/// Blood Bend X. Deal X damage. Triple X if it is equal to (or more than) the number of other cards in your Hand.
/// </summary>
public class SeelsExsanguination() : SeelsTheRecklessCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public const int Multiplier = 3;

    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
    ];

    private bool IsTripled(int x)
    {
        var otherCards = PileType.Hand.GetPile(Owner).Cards.Count(c => c != this);
        return IsUpgraded ? x >= otherCards : x == otherCards;
    }

    protected override bool ShouldGlowGoldInternal => IsTripled(SeelsMechanicsHelper.GetBlood(Owner.Creature));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);
        if (IsTripled(x)) x *= Multiplier;

        await DamageCmd.Attack(x)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }
}
