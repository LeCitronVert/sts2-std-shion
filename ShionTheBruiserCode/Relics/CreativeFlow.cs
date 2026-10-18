using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using Random = ShionTheBruiser.ShionTheBruiserCode.Powers.Random;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Upgraded Creative Spark. ALL your cards and the ones you create gain Novel.
/// Gain 1 additional Random when you play a Novel card. Start each combat with 5 Random.
/// </summary>
public class CreativeFlow : SlalTheEndlessRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("NovelRandom", 1),
        new DynamicVar("StartingRandom", 5),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Random>(),
    ];

    // Novel is removed when the card is played, so remember it before the play resolves.
    private CardModel? _novelCardBeingPlayed;

    public override async Task BeforeCombatStart()
    {
        foreach (var cardModel in Owner.PlayerCombatState?.AllCards ?? [])
        {
            cardModel.AddKeyword(SlalCardKeywords.Novel);
        }

        Flash();
        await SlalMechanicsHelper.GainRandom(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["StartingRandom"].IntValue);
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        var novelPower = Owner.Creature.GetPower<NovelTracker>();
        if (card.Owner == Owner && novelPower != null && !novelPower.CardsUsedThisFight.Contains(card.Id))
        {
            card.AddKeyword(SlalCardKeywords.Novel);
        }

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        SlalMechanicsHelper.EnsureCreatureHasNovelTracker(Owner.Creature);

        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner && cardPlay.Card.Keywords.Contains(SlalCardKeywords.Novel))
        {
            _novelCardBeingPlayed = cardPlay.Card;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_novelCardBeingPlayed != cardPlay.Card) return;
        _novelCardBeingPlayed = null;

        Flash();
        await SlalMechanicsHelper.GainRandom(choiceContext, Owner.Creature, DynamicVars["NovelRandom"].IntValue);
    }
}
