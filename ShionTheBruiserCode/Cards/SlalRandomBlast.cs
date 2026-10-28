using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team Blast. Whenever ANYONE plays a card this turn, apply 1 (3) Confusion to a random enemy and increase this value by 1.
/// </summary>
public class SlalRandomBlast() : SlalTheEndlessCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.TeamBlast];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Confusion>(1),
        new DynamicVar("Increase", RandomBlast.BaseIncrease),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var power = await PowerCmd.Apply<RandomBlast>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["Confusion"].BaseValue,
            card.Owner.Creature,
            card
        );
        power?.SkipPlayOf(card);

        await DreamersMechanicsHelper.ExhaustAllTeamBlasts(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Confusion"].UpgradeValueBy(2);
    }
}
