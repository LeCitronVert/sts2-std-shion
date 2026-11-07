using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Events;

/// <summary>
/// The Gambler. Any act. Appears once Seels' Ascension 2 is unlocked by anyone in the run
/// (= "Roll With The Punches" revealed: win a run in Ascension 1 with Seels), then for any character.
/// Every player needs 120+ gold (like Endless Conveyor).
///
/// Do not play: transform a card of your choice into the current curse (Shame at first), then leave.
///   When the current curse is Great Fortunes, it becomes "Celebrate": add Great Fortunes to your Deck, then leave.
/// Play once (40 gold, "Broke" when you can't pay): roll a new current curse among the 13 below, never one
///   already revealed (Shame counts as revealed) until all of them have been.
/// Play until you win: lose ALL your gold, obtain the Cursed Dice, then leave.
/// </summary>
public sealed class TheGambler : DreamersEventModel
{
    private const string CurseNameKey = "CurseName";
    private const string ResultKey = "Result";

    private static IReadOnlyList<CardModel> Curses =>
    [
        ModelDb.Card<BadLuck>(),
        ModelDb.Card<Clumsy>(),
        ModelDb.Card<Debt>(),
        ModelDb.Card<Decay>(),
        ModelDb.Card<Doubt>(),
        ModelDb.Card<Folly>(),
        ModelDb.Card<Greed>(),
        ModelDb.Card<Guilty>(),
        ModelDb.Card<Injury>(),
        ModelDb.Card<PoorSleep>(),
        ModelDb.Card<Regret>(),
        ModelDb.Card<Shame>(),
        ModelDb.Card<SeelsGreatFortunes>(),
    ];

    private CardModel? _currentCurse;
    // Replaced, never mutated: a mutable copy of the event shares its fields' references with the canonical one
    private IReadOnlyList<ModelId> _revealed = [];
    private int _plays;

    private CardModel CurrentCurse
    {
        get => _currentCurse ?? ModelDb.Card<Shame>();
        set
        {
            AssertMutable();
            _currentCurse = value;
        }
    }

    private bool IsLucky => CurrentCurse is SeelsGreatFortunes;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new GoldVar(40),
        new StringVar(CurseNameKey),
        new StringVar(ResultKey),
    ];

    public override bool IsAllowed(IRunState runState) =>
        runState.UnlockState.IsEpochRevealed<SeelsRollWithThePunchesEpoch>()
        && runState.Players.All(p => p.Gold >= 120);

    public override void CalculateVars()
    {
        ((StringVar)DynamicVars[CurseNameKey]).StringValue = CurrentCurse.Title;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => Options();

    private IReadOnlyList<EventOption> Options() =>
    [
        QuitOption(),
        PlayOnceOption(),
        new EventOption(this, PlayUntilYouWin, $"{Id.Entry}.pages.ALL.options.PLAY_UNTIL_YOU_WIN", HoverTipFactory.FromRelic<CursedDice>()),
    ];

    private EventOption QuitOption() => IsLucky
        ? new EventOption(this, Celebrate, $"{Id.Entry}.pages.ALL.options.CELEBRATE", HoverTipFactory.FromCardWithCardHoverTips<SeelsGreatFortunes>())
        : new EventOption(this, DoNotPlay, $"{Id.Entry}.pages.ALL.options.DO_NOT_PLAY",
            HoverTipFactory.FromCard(CurrentCurse), HoverTipFactory.Static(StaticHoverTip.Transform));

    private EventOption PlayOnceOption() => (Owner?.Gold ?? 0) >= DynamicVars.Gold.IntValue
        ? new EventOption(this, PlayOnce, $"{Id.Entry}.pages.ALL.options.PLAY_ONCE")
        : new EventOption(this, null, $"{Id.Entry}.pages.ALL.options.BROKE");

    private async Task DoNotPlay()
    {
        var player = Owner!;
        var picked = (await CardSelectCmd.FromDeckForTransformation(player, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1))).FirstOrDefault();
        if (picked != null)
        {
            await CardCmd.Transform(picked, player.RunState.CreateCard(CurrentCurse, player), CardPreviewStyle.EventLayout);
        }
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.DO_NOT_PLAY.description"));
    }

    private async Task Celebrate()
    {
        var player = Owner!;
        var card = player.RunState.CreateCard(ModelDb.Card<SeelsGreatFortunes>(), player);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.CELEBRATE.description"));
    }

    private async Task PlayOnce()
    {
        await PlayerCmd.LoseGold(DynamicVars.Gold.IntValue, Owner!, GoldLossType.Spent);
        RollCurse();

        // The result line needs CurseName resolved before it goes into the page text
        // L10NLookup returns a bare LocString: the event's vars must be added before formatting
        var resultLine = L10NLookup($"{Id.Entry}.{(IsLucky ? "LUCKY" : "QUITTER")}");
        DynamicVars.AddTo(resultLine);
        ((StringVar)DynamicVars[ResultKey]).StringValue = resultLine.GetFormattedText();

        var page = _plays == 0 ? "PLAY_ONCE_FIRST" : "PLAY_ONCE_AGAIN";
        _plays++;
        SetEventState(L10NLookup($"{Id.Entry}.pages.{page}.description"), Options());
    }

    private void RollCurse()
    {
        AssertMutable();
        // Shame, the starting curse, counts as revealed: 12 possible curses on the first roll
        if (_plays == 0) _revealed = [CurrentCurse.Id];

        var unrevealed = Curses.Where(c => !_revealed.Contains(c.Id)).ToList();
        if (unrevealed.Count == 0)
        {
            // All 13 were revealed once: start over, without repeating the current one right away
            _revealed = [CurrentCurse.Id];
            unrevealed = Curses.Where(c => c.Id != CurrentCurse.Id).ToList();
        }
        CurrentCurse = Rng.NextItem(unrevealed)!;
        _revealed = [.._revealed, CurrentCurse.Id];
        ((StringVar)DynamicVars[CurseNameKey]).StringValue = CurrentCurse.Title;
    }

    private async Task PlayUntilYouWin()
    {
        var player = Owner!;
        if (player.Gold > 0)
        {
            await PlayerCmd.LoseGold(player.Gold, player, GoldLossType.Spent);
        }
        await RelicCmd.Obtain(ModelDb.Relic<CursedDice>().ToMutable(), player);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.PLAY_UNTIL_YOU_WIN.description"));
    }
}
