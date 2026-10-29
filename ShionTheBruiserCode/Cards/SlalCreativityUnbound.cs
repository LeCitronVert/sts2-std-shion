using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Always Novel. Add 1 random power out of Genie, Television and Sparrow Form into your Hand. It is free to play this turn. Exhaust.
/// </summary>
public class SlalCreativityUnbound() : SlalTheEndlessCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        SlalCardKeywords.AlwaysNovel,
        CardKeyword.Exhaust,
    ];

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

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalGenieForm>(),
        HoverTipFactory.FromCard<SlalTelevisionForm>(),
        HoverTipFactory.FromCard<SlalSparrowForm>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);

        var power = SlalMechanicsHelper.CreateTokenAvoiding(card.Owner, SlalMechanicsHelper.CreativityUnboundTokenPowers, LastGivenPower, false);
        LastGivenPower = power.Id;
        // Made free before being added, so Sparrow Form doesn't trigger on it
        power.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(power, PileType.Hand, card.Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
