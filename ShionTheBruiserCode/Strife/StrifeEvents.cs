using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Shared base of the Act 4 Architect cutscenes: Combat layout (the Architect stands in his victory room), one speech
/// bubble per line, a button to go to the next line. Lines are built for the local player's character (text only).
/// ICustomModel: BaseLib then gives the Id the mod prefix, which the loc keys use (SHIONTHEBRUISER-...).
/// </summary>
public abstract class StrifeDialogueEvent : EventModel, BaseLib.Abstracts.ICustomModel
{
    protected const string Table = "events";

    private static readonly LocString EmptyLocString = new("ancients", "PROCEED.description");

    private List<LocString>? _lines;
    private int _lineIndex;
    private NSpeechBubbleVfx? _bubble;

    public override EventLayoutType LayoutType => EventLayoutType.Combat;

    public override IEnumerable<LocString> GameInfoOptions => [];

    protected abstract List<LocString> BuildLines();

    /// <summary>Loc key of the button shown on the last line.</summary>
    protected abstract string LastOptionKey { get; }

    protected abstract Task OnLastOption();

    private List<LocString> Lines => _lines ??= BuildLines();

#if STS2_BETA
    private Creature? Architect => _combatSynchronizer?.CombatStateForLayout?.Enemies.FirstOrDefault();
#else
    private Creature? Architect => _combatStateForCombatLayout?.Enemies.FirstOrDefault();
#endif

    protected override void SetInitialEventState(bool isPreFinished) => SetEventState(EmptyLocString, GenerateInitialOptionsWrapper());

    /// <summary>
    /// In a shared event every option needs all players' votes. So in multiplayer the lines play on their own, one
    /// bubble after another (local only), and the only option is the last one (Fight), which everyone votes for so
    /// they all enter the fight together. Solo: one Continue click per line.
    /// </summary>
    private bool AutoAdvance => IsShared && Owner?.RunState.Players.Count > 1;

    private bool _finished;

    /// <summary>How long each auto-advanced line stays, from its length.</summary>
    private const float SecondsPerChar = 0.06f;
    private const float MinLineTime = 2.2f;
    private const float MaxLineTime = 6f;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        _lineIndex = 0;
        _finished = false;
        return [AutoAdvance ? LastOption() : OptionForCurrentLine()];
    }

    public override void OnRoomEnter()
    {
        base.OnRoomEnter();
        TaskHelper.RunSafely(ShowFirstLine());
    }

    // Like the base game's Architect: a short pause, so the room (and his node) is there before he speaks
    private async Task ShowFirstLine()
    {
        await Cmd.Wait(0.75f);
        if (_lineIndex != 0 || _finished) return;
        ShowCurrentLine();
        if (!AutoAdvance) return;

        // Multiplayer: the next lines follow by themselves (visual only, every client at its own pace)
        while (!_finished && _lineIndex < Lines.Count - 1)
        {
            var length = Lines[_lineIndex].GetFormattedText().Length;
            await Cmd.Wait(Math.Clamp(length * SecondsPerChar, MinLineTime, MaxLineTime));
            if (_finished) return;
            _lineIndex++;
            ShowCurrentLine();
        }
    }

    private EventOption LastOption() =>
        new EventOption(this, Finish, new LocString(Table, LastOptionKey + ".title"), EmptyLocString,
            LastOptionKey, Array.Empty<IHoverTip>()).ThatWontSaveToChoiceHistory();

    private EventOption OptionForCurrentLine()
    {
        var isLast = _lineIndex >= Lines.Count - 1;
        var key = isLast ? LastOptionKey : $"{Id.Entry}.options.CONTINUE";
        return new EventOption(this, isLast ? Finish : Advance, new LocString(Table, key + ".title"), EmptyLocString,
            key, Array.Empty<IHoverTip>()).ThatWontSaveToChoiceHistory();
    }

    private Task Advance()
    {
        _lineIndex++;
        ShowCurrentLine();
        SetEventState(EmptyLocString, [OptionForCurrentLine()]);
        return Task.CompletedTask;
    }

    private async Task Finish()
    {
        _finished = true;
        HideBubble();
        await OnLastOption();
    }

    private void ShowCurrentLine()
    {
        if (Owner == null || !LocalContext.IsMe(Owner)) return;
        HideBubble();
        if (_lineIndex >= Lines.Count || Architect is not { } architect) return;
        _bubble = TalkCmd.Play(Lines[_lineIndex], architect, VfxColor.DarkGray, VfxDuration.Forever);
    }

    private void HideBubble()
    {
        if (_bubble == null) return;
        TaskHelper.RunSafely(_bubble.AnimOut());
        _bubble = null;
    }

    protected LocString Line(string key) => new(Table, $"{Id.Entry}.lines.{key}");

    protected LocString RandomLine(string key, int count) => Line($"{key}.{Rng.NextInt(count)}");

    /// <summary>The local player's class name without its leading "The" ("Ironclad", "Bruiser").</summary>
    public static string ClassName(CharacterModel character)
    {
        var title = character.Title.GetFormattedText().Trim();
        return title.StartsWith("The ", StringComparison.OrdinalIgnoreCase) ? title[4..].Trim() : title;
    }

    public static bool IsDreamersCharacter(CharacterModel character) =>
        character is Character.ShionTheBruiser or SlalTheEndless or SeelsTheReckless or FryTheCommander;

    protected static CharacterModel LocalCharacter(EventModel model) =>
        model.Owner is { } owner ? (LocalContext.GetMe(owner.RunState.Players)?.Character ?? owner.Character) : ModelDb.Character<Ironclad>();
}

/// <summary>
/// Act 4's boss node: the Architect's opening lines, then the fight (<see cref="StrifeArchitectEncounter"/>).
/// Entered instead of the boss combat room (<see cref="StrifeRoomPatch"/>).
/// </summary>
public sealed class StrifeArchitectIntro : StrifeDialogueEvent
{
    public override bool IsShared => true;

    public override EncounterModel CanonicalEncounter => ModelDb.Encounter<StrifeArchitectEncounter>();

    protected override string LastOptionKey => $"{Id.Entry}.options.FIGHT";

    protected override List<LocString> BuildLines()
    {
        var character = LocalCharacter(this);
        var className = ClassName(character);

        LocString fine;
        if (IsDreamersCharacter(character))
        {
            fine = Line("FINE_DREAMERS");
            fine.Add("FirstName", character.TitleObject.GetFormattedText());
        }
        else
        {
            fine = Line("FINE");
        }
        fine.Add("ClassName", className);

        return
        [
            RandomLine("GREETING", 3),
            RandomLine("WONDER", 3),
            Line("UNLESS"),
            Line("GEMS"),
            fine,
            Line("MODICUM"),
            Line($"THREAT.{ThreatKey(character)}"),
        ];
    }

    /// <summary>Per-character key suffix for Act 4 lines (also used by the Act 4 co-op pings).</summary>
    internal static string ThreatKey(CharacterModel character) => character switch
    {
        Ironclad => "IRONCLAD",
        Silent => "SILENT",
        Defect => "DEFECT",
        Regent => "REGENT",
        Necrobinder => "NECROBINDER",
        Character.ShionTheBruiser => "SHION",
        SlalTheEndless => "SLAL",
        SeelsTheReckless => "SEELS",
        FryTheCommander => "FRY",
        _ => "OTHER",
    };

    protected override Task OnLastOption()
    {
        EnterCombatWithoutExitingEvent(ModelDb.Encounter<StrifeArchitectEncounter>().ToMutable(), Array.Empty<Reward>(), shouldResumeAfterCombat: false);
        StrifeMusic.Refresh();
        return Task.CompletedTask;
    }
}

/// <summary>
/// After the Act 4 Architect is "defeated": the end-of-playthrough lines (placeholder, will be character dependent),
/// then the true victory (<see cref="StrifeVictory"/>).
/// </summary>
public sealed class StrifeEnding : StrifeDialogueEvent
{
    public override EncounterModel CanonicalEncounter => ModelDb.Encounter<TheArchitectEventEncounter>();

    protected override string LastOptionKey => $"{Id.Entry}.options.END";

    protected override List<LocString> BuildLines() => [Line("0"), Line("1"), Line("2")];

    protected override async Task OnLastOption()
    {
        if (Owner == null) return;
        if (Owner.RunState.Players.Count > 1)
        {
            MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.SetWaitingForOtherPlayersOverlayVisible(visible: true);
        }
        // Like the base game's Architect: every player clicks, then RunManager.EnterNextAct ends the run for everyone
        RunManager.Instance.ActChangeSynchronizer.SetLocalPlayerReady();
        await Task.CompletedTask;
    }
}

/// <summary>Act 4's boss node opens the Architect's intro instead of going straight into the fight.</summary>
[HarmonyPatch(typeof(RunManager), "CreateRoom")]
public static class StrifeRoomPatch
{
    [HarmonyPrefix]
    public static bool Prefix(RoomType roomType, MapPointType mapPointType, AbstractModel? model, ref AbstractRoom __result)
    {
        if (roomType != RoomType.Boss || model != null || !StrifeAct.IsStrife(StrifeRun.State)) return true;
        __result = new EventRoom(ModelDb.Event<StrifeArchitectIntro>());
        return false;
    }
}

/// <summary>The ending room counts as the victory room (game over screen shows a win, run counts as won).</summary>
[HarmonyPatch(typeof(AbstractRoom), nameof(AbstractRoom.IsVictoryRoom), MethodType.Getter)]
public static class StrifeVictoryRoomPatch
{
    [HarmonyPostfix]
    public static void Postfix(AbstractRoom __instance, ref bool __result)
    {
        if (!__result && __instance is EventRoom { CanonicalEvent: StrifeEnding }) __result = true;
    }
}
