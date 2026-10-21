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
/// Add 2 Slashes (Slashes+) into your Hand. Gamble 2. All Slashes now hit a number of times equal to their energy cost +1.
/// </summary>
public class SeelsUntelegraphAttacks() : SeelsTheRecklessCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<UntelegraphAttacks>(1M),
        new CardsVar(2),
        new DynamicVar("Gamble", 2M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SeelsSlash>(IsUpgraded),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Gamble),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.Erratic),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await SeelsMechanicsHelper.AddSlashes(Owner, DynamicVars.Cards.IntValue, PileType.Hand, IsUpgraded);
        await SeelsMechanicsHelper.Gamble(choiceContext, Owner, DynamicVars["Gamble"].IntValue, this);
        await PowerCmd.Apply<UntelegraphAttacks>(choiceContext, Owner.Creature, DynamicVars["UntelegraphAttacks"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
    }
}
