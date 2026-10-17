using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalFrontflipAndAHalf() : SlalTheEndlessCard(2,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    private static IReadOnlyList<CardModel> PowerOptions => [
        ModelDb.Card<SlalBumperForm>(),
        ModelDb.Card<SlalVentilatorForm>(),
        ModelDb.Card<SlalSqueakySquirrelForm>(),
        ModelDb.Card<SlalSlideForm>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5M, ValueProp.Move),
        new PowerVar<BlockNextTurnPower>(5),
        new PowerVar<Confusion>(5),
        new CardsVar(2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromCard<SlalBumperForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalVentilatorForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalSqueakySquirrelForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalSlideForm>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
        await PowerCmd.Apply<BlockNextTurnPower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["BlockNextTurnPower"].BaseValue,
            card.Owner.Creature,
            card
        );
        await PowerCmd.Apply<Confusion>(
            choiceContext,
            play.Target,
            card.DynamicVars["Confusion"].BaseValue,
            card.Owner.Creature,
            card
        );

        var powers = PowerOptions
            .TakeRandom(card.DynamicVars.Cards.IntValue, card.Owner.RunState.Rng.CombatCardGeneration)
            .Select(p =>
            {
                var power = card.CombatState!.CreateCard(p, card.Owner);
                if (IsUpgraded && power.IsUpgradable) CardCmd.Upgrade(power);
                return power;
            })
            .ToList();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(powers, PileType.Draw, card.Owner, CardPilePosition.Random));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2M);
    }
}
