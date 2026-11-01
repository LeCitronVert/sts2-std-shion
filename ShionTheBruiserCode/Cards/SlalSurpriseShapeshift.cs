using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Apply 2 (3) Confusion. Gain 1 Flashback. Shapeshift (twice).
/// </summary>
public class SlalSurpriseShapeshift() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Common,
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
        new PowerVar<Confusion>(2),
        new PowerVar<Flashback>(1),
        new CardsVar(1),
    ];

    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Shapeshift];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromPower<Flashback>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Shapeshift),
        HoverTipFactory.FromKeyword(SlalCardKeywords.GoofballTokenPower),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);
        await SlalMechanicsHelper.GainFlashback(choiceContext, card.Owner.Creature, card.DynamicVars["Flashback"].BaseValue, card);

        for (var i = 0; i < card.DynamicVars.Cards.IntValue; i++)
        {
            var power = SlalMechanicsHelper.CreateGoofballTokenPower(card.Owner, false, LastGivenPower);
            LastGivenPower = power.Id;
            await SlalMechanicsHelper.AddShapeshiftedPower(choiceContext, card.Owner, power);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Confusion"].UpgradeValueBy(1);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
