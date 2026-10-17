using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalSurpriseShapeshift() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    private ModelId? _lastGivenPower;

    /// <summary>
    /// Remembers the last power given so the next one is different.
    /// </summary>
    private ModelId? LastGivenPower
    {
        get => _lastGivenPower;
        set
        {
            AssertMutable();
            _lastGivenPower = value;
        }
    }

    private static IReadOnlyList<CardModel> PowerOptions => [
        ModelDb.Card<SlalBumperForm>(),
        ModelDb.Card<SlalVentilatorForm>(),
        ModelDb.Card<SlalSqueakySquirrelForm>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5M, ValueProp.Move),
        new PowerVar<Confusion>(5),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromCard<SlalBumperForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalVentilatorForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalSqueakySquirrelForm>(IsUpgraded),
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
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        await PowerCmd.Apply<Confusion>(
            choiceContext,
            play.Target,
            card.DynamicVars["Confusion"].BaseValue,
            card.Owner.Creature,
            card
        );

        var power = SlalMechanicsHelper.CreateTokenAvoiding(card.Owner, PowerOptions, LastGivenPower, IsUpgraded);
        LastGivenPower = power.Id;
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(power, PileType.Draw, card.Owner, CardPilePosition.Random));
    }

    protected override void OnUpgrade()
    {
    }
}
