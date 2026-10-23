using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 (2) Memory this turn. Upgrade a random card (a card of your choice) in your Hand.
/// </summary>
public class SlalProofReading() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<MemoryThisTurn>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Memory>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await SlalMechanicsHelper.GainMemoryThisTurn(choiceContext, card.Owner.Creature, card.DynamicVars["MemoryThisTurn"].BaseValue, card);

        var toUpgrade = IsUpgraded
            ? await CardSelectCmd.FromHandForUpgrade(choiceContext, card.Owner, card)
            : PileType.Hand.GetPile(card.Owner).Cards
                .Where(c => c.IsUpgradable)
                .TakeRandom(1, card.Owner.RunState.Rng.CombatCardSelection)
                .FirstOrDefault();

        if (toUpgrade != null)
        {
            CardCmd.Upgrade(toUpgrade);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MemoryThisTurn"].UpgradeValueBy(1);
    }
}
