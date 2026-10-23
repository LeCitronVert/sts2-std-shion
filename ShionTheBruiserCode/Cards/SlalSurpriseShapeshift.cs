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

/// <summary>
/// Deal 5 damage. Apply 5 Confusion. Add a random Goofball Token Power into your Draw Pile.
/// </summary>
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

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5M, ValueProp.Move),
        new PowerVar<Confusion>(5),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
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

        var power = SlalMechanicsHelper.CreateGoofballTokenPower(card.Owner, IsUpgraded, LastGivenPower);
        LastGivenPower = power.Id;
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(power, PileType.Draw, card.Owner, CardPilePosition.Random));
    }

    protected override void OnUpgrade()
    {
    }
}
