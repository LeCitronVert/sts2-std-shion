using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 5 Confusion. Add a random (upgraded) Colorless card into your Hand.
/// </summary>
public class SlalPuzzlingTip() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Confusion>(5),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);

        var colorless = ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(card.Owner.UnlockState, card.Owner.RunState.CardMultiplayerConstraint);
        foreach (var newCard in SlalMechanicsHelper.CreateRandomCards(card.Owner, 1, IsUpgraded, colorless))
        {
            await CardPileCmd.AddGeneratedCardToCombat(newCard, PileType.Hand, card.Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
