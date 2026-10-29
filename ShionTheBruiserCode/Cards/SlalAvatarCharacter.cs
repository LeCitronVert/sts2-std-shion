using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// TD Fact: apply 3 Confusion. Increase this card's Confusion by 1 (2) each time you play a Novel card this combat. Exhaust.
/// Only the Novel cards played while this card is in combat count.
/// (The class keeps its old name so existing saves still find it.)
/// </summary>
//todo : type not specified
public class SlalAvatarCharacter() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Confusion>(3),
        new DynamicVar("Increase", 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
    ];

    private decimal _extraConfusionFromNovel;

    /// <summary>
    /// Required so we can restore the extra Confusion after a downgrade (see Rampage).
    /// </summary>
    private decimal ExtraConfusionFromNovel
    {
        get => _extraConfusionFromNovel;
        set
        {
            AssertMutable();
            _extraConfusionFromNovel = value;
        }
    }

    /// <summary>Novel is removed when the card is played, so remember it before the play resolves.</summary>
    private HashSet<CardModel> _novelCardsBeingPlayed = [];

    // MemberwiseClone shares the set with the canonical model, so give each instance its own.
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _novelCardsBeingPlayed = [];
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        await base.BeforeCardPlayed(cardPlay);
        if (cardPlay.Card.Owner == Owner && cardPlay.IsFirstInSeries && SlalMechanicsHelper.IsNovel(cardPlay.Card))
        {
            _novelCardsBeingPlayed.Add(cardPlay.Card);
        }
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.IsLastInSeries && _novelCardsBeingPlayed.Remove(cardPlay.Card))
        {
            var increase = DynamicVars["Increase"].BaseValue;
            DynamicVars["Confusion"].BaseValue += increase;
            ExtraConfusionFromNovel += increase;
        }
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<Confusion>(choiceContext, play.Target, card.DynamicVars["Confusion"].BaseValue, card.Owner.Creature, card);
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars["Confusion"].BaseValue += ExtraConfusionFromNovel;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(1);
    }
}
