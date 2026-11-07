using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Events;

/// <summary>
/// Monster Soul. Any act. Appears once Shion's Ascension 2 is unlocked by anyone in the run
/// (= "If One Doesn't Cut It..." revealed: win a run in Ascension 1 with Shion), then for any character.
/// Hold onto the Crystal: lose 10 Max HP, obtain Monstermancy.
/// Smash the Crystal: heal to full HP, add a Regret to your Deck.
/// The outcome text depends on who picks: Shion, another Dreamer (Slal, Fry, Seels) or anyone else.
/// </summary>
public sealed class MonsterSoul : DreamersEventModel
{
    private const string MaxHpLossKey = "MaxHpLoss";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(MaxHpLossKey, 10m)];

    public override bool IsAllowed(IRunState runState) =>
        runState.UnlockState.IsEpochRevealed<ShionIfOneDoesntCutItEpoch>();

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Hold, $"{Id.Entry}.pages.INITIAL.options.HOLD", HoverTipFactory.FromCardWithCardHoverTips<ShionMonstermancy>())
            .ThatDecreasesMaxHp(DynamicVars[MaxHpLossKey].BaseValue),
        new EventOption(this, Smash, $"{Id.Entry}.pages.INITIAL.options.SMASH", HoverTipFactory.FromCardWithCardHoverTips<Regret>()),
    ];

    private async Task Hold()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars[MaxHpLossKey].BaseValue, isFromCard: false);
        var card = Owner.RunState.CreateCard(ModelDb.Card<ShionMonstermancy>(), Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.HOLD_{WhoSuffix}.description"));
    }

    private async Task Smash()
    {
        var creature = Owner!.Creature;
        if (creature.CurrentHp < creature.MaxHp)
        {
            await CreatureCmd.Heal(creature, creature.MaxHp - creature.CurrentHp);
        }
        await CardPileCmd.AddCurseToDeck<Regret>(Owner);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.SMASH_{WhoSuffix}.description"));
    }

    private string WhoSuffix => Owner!.Character switch
    {
        Character.ShionTheBruiser => "SHION",
        SlalTheEndless or FryTheCommander or SeelsTheReckless => "DREAMER",
        _ => "OTHER",
    };
}
