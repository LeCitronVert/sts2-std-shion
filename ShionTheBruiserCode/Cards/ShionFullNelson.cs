using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionFullNelson() : ShionDefensiveCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Defensive];
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = play.Card;
        var applier = play.Card.Owner.Creature;
        var target = play.Target;
        ArgumentNullException.ThrowIfNull(target);
        
        await PowerCmd.Apply<FullNelson>(
            choiceContext,
            applier,
            1,
            applier,
            card
        );
        await PowerCmd.Apply<FullNelson>(
            choiceContext,
            target,
            1,
            applier,
            card
        );
        
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}