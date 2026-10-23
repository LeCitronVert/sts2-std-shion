using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Whenever you play a card that puts a card into your Draw Pile, this enemy gains 5 Confusion.
/// </summary>
// Trashed for now: kept in the code, but out of rewards, shops and random generation.
[Pool(typeof(TokenCardPool))]
public class SlalAnchorThrow() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Token,
    TargetType.AnyEnemy)
{
    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<AnchoredDown>(5),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<AnchoredDown>(choiceContext, play.Target, card.DynamicVars["AnchoredDown"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
