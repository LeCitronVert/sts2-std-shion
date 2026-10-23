using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Power with no lasting effect: add 1 random of Walking Encyclopedia, Television or Sparrow Form into your Hand.
/// </summary>
public class SlalCreativityUnbound() : SlalTheEndlessCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
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
        ModelDb.Card<SlalWalkingEncyclopedia>(),
        ModelDb.Card<SlalTelevisionForm>(),
        ModelDb.Card<SlalSparrowForm>(),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalWalkingEncyclopedia>(IsUpgraded),
        HoverTipFactory.FromCard<SlalTelevisionForm>(IsUpgraded),
        HoverTipFactory.FromCard<SlalSparrowForm>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);

        var power = SlalMechanicsHelper.CreateTokenAvoiding(card.Owner, PowerOptions, LastGivenPower, IsUpgraded);
        LastGivenPower = power.Id;
        await CardPileCmd.AddGeneratedCardToCombat(power, PileType.Hand, card.Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
