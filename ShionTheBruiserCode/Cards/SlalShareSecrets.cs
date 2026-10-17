using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalShareSecrets() : SlalTheEndlessCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10M, ValueProp.Move),
        new PowerVar<Confusion>(5),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromCard<SlalFreshIdea>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);

        var drawResult = await CardPileCmd.AddGeneratedCardToCombat(SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, IsUpgraded), PileType.Draw, card.Owner, CardPilePosition.Random);
        var discardResult = await CardPileCmd.AddGeneratedCardToCombat(SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, IsUpgraded), PileType.Discard, card.Owner);
        await CardPileCmd.AddGeneratedCardToCombat(SlalMechanicsHelper.CreateCard<SlalFreshIdea>(card.Owner, IsUpgraded), PileType.Hand, card.Owner);
        CardCmd.PreviewCardPileAdd([drawResult, discardResult]);
    }

    protected override void OnUpgrade()
    {
    }
}
