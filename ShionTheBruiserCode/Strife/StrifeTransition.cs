using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Hooks Act 4 into the end of the run. RunManager.EnterNextAct runs on every client once every player is done with the
/// current room, so everything here happens in the same order everywhere:
/// <list type="bullet">
/// <item>After the Act 3 Architect, if every player holds a combo Chaos Emerald: the Chaos Emeralds cutscene, every
/// missing Chaos Emerald relic, Super Form, then Act 4 is added to the run and entered.</item>
/// <item>After the Act 4 boss: the ending room (<see cref="StrifeEnding"/>).</item>
/// <item>After the ending room: the true victory.</item>
/// </list>
/// </summary>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterNextAct))]
public static class StrifeTransition
{
    [HarmonyPrefix]
    public static bool Prefix(ref Task __result)
    {
        var state = StrifeRun.State;
        if (state == null || state.CurrentActIndex < state.Acts.Count - 1) return true;

        var room = state.CurrentRoom;
        if (room is EventRoom { CanonicalEvent: StrifeEnding })
        {
            __result = StrifeVictory.WinRun(state);
            return false;
        }
        if (StrifeAct.IsStrife(state))
        {
            __result = EnterEnding();
            return false;
        }
        if (room is EventRoom { CanonicalEvent: TheArchitect } && StrifeRun.IsStrifeUnlocked(state)
            && state.Acts.All(a => !StrifeAct.IsStrife(a)))
        {
            __result = EnterStrife(state);
            return false;
        }
        return true;
    }

    private static async Task EnterStrife(RunState state)
    {
        var cutscene = StrifeCutscene.Start(state);
        // The Architect's room shows "Waiting for other players..." in multiplayer once you're done: not during the cutscene
        MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.SetWaitingForOtherPlayersOverlayVisible(visible: false);
        {
            if (cutscene != null) await cutscene.FadeToBlack();
            StrifeMusic.CutsceneActive = true;
            StrifeMusic.Refresh();

            await Cmd.Wait(2f);
            cutscene?.EmergeEmeralds();
            await Cmd.Wait(1.2f);

            // One by one, every Chaos Emerald relic each player doesn't have yet (in any form)
            foreach (var color in ChaosEmeraldHelper.AllColors)
            {
                cutscene?.Pulse(color);
                foreach (var player in state.Players)
                {
                    if (player.Relics.Any(r => r is ChaosEmeraldRelic emerald && emerald.Color == color)) continue;
                    await RelicCmd.Obtain(ChaosEmeraldHelper.SoloRelic(color).ToMutable(), player);
                }
                await Cmd.Wait(0.55f);
            }

            await Cmd.Wait(0.6f);
            if (cutscene != null) await cutscene.Converge();
            StrifeMusic.PlayTransformSfx();
            cutscene?.Flash();

            foreach (var player in state.Players)
            {
                await RelicCmd.Obtain(ModelDb.Relic<SuperForm>().ToMutable(), player);
            }
            await Cmd.Wait(2f);

            AddStrifeAct(state);
        }

        StrifeMusic.CutsceneActive = false;
        await RunManager.Instance.EnterAct(state.Acts.Count - 1);
        cutscene?.FadeOutWhite();
        StrifeMusic.Refresh();
    }

    /// <summary>Appends Act 4 to the run. Its rooms use their own RNG stream, so no other roll is shifted.</summary>
    public static void AddStrifeAct(RunState state)
    {
        var act = ModelDb.Act<StrifeAct>().ToMutable();
        act.GenerateRooms(new Rng(state.Rng.Seed, "strife"), state.UnlockState, state.Players.Count > 1);
        StrifeRun.SetActs(state, [.. state.Acts, act]);
    }

    /// <summary>Like the base game entering the Architect's room after the last boss.</summary>
    private static async Task EnterEnding()
    {
        using (new NetLoadingHandle(RunManager.Instance.NetService))
        {
            if (TestMode.IsOff) await NGame.Instance.Transition.RoomFadeOut();
            StrifeRun.ClearScreens();
            await RunManager.Instance.EnterRoom(new EventRoom(ModelDb.Event<StrifeEnding>()));
            await StrifeRun.FadeIn();
        }
    }
}

/// <summary>
/// Since the v0.111.1 hotfix / current beta, the Architect's "Proceed" calls RunManager.WinRun directly (local, no
/// ActChangeSynchronizer), so <see cref="StrifeTransition"/> never ran. When Act 4 is unlocked, turn that click back
/// into the old "player ready" vote: once every player is ready, ActChangeSynchronizer calls EnterNextAct on every
/// client, and StrifeTransition plays the cutscene there, in sync.
/// Patches TheArchitect.WinRun itself (not RunManager.WinRun): the new version clears the event's options after the
/// win, which would leave a finished event (and the room's Proceed button) on screen during the cutscene.
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "WinRun")]
public static class StrifeArchitectWinRunPatch
{
    [HarmonyPrefix]
    public static bool Prefix(TheArchitect __instance, ref Task __result)
    {
        var state = StrifeRun.State;
        if (state == null || !StrifeRun.IsStrifeUnlocked(state) || state.Acts.Any(a => StrifeAct.IsStrife(a)))
            return true;

        __result = OldWinRun(__instance);
        return false;
    }

    private static readonly Func<TheArchitect, AncientDialogue?> DialogueGetter =
        AccessTools.MethodDelegate<Func<TheArchitect, AncientDialogue?>>(AccessTools.PropertyGetter(typeof(TheArchitect), "Dialogue"));

    private static readonly Func<TheArchitect, ArchitectAttackers, Task<bool>> AnimPlayerAttack =
        AccessTools.MethodDelegate<Func<TheArchitect, ArchitectAttackers, Task<bool>>>(
            AccessTools.Method(typeof(TheArchitect), "AnimPlayerAttackIfNecessary"));

    private static readonly Func<TheArchitect, ArchitectAttackers, Task<bool>> AnimArchitectAttack =
        AccessTools.MethodDelegate<Func<TheArchitect, ArchitectAttackers, Task<bool>>>(
            AccessTools.Method(typeof(TheArchitect), "AnimArchitectAttackIfNecessary"));

    /// <summary>The pre-update TheArchitect.WinRun.</summary>
    private static async Task OldWinRun(TheArchitect architect)
    {
        if (!LocalContext.IsMe(architect.Owner)) return;
        if (DialogueGetter(architect) is { } dialogue)
        {
            await AnimPlayerAttack(architect, dialogue.EndAttackers);
            await AnimArchitectAttack(architect, dialogue.EndAttackers);
        }
        if (architect.Owner!.RunState.Players.Count > 1)
        {
            MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.SetWaitingForOtherPlayersOverlayVisible(visible: true);
        }
        RunManager.Instance.ActChangeSynchronizer.SetLocalPlayerReady();
    }
}

/// <summary>The run's true ending, after <see cref="StrifeEnding"/>.</summary>
public static class StrifeVictory
{
    /// <summary>Set once the Act 4 run is won, for the game over screen (banner, text, confetti).</summary>
    public static bool IsStrifeWin { get; private set; }

    public static async Task WinRun(RunState state)
    {
        IsStrifeWin = true;
        MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.SetWaitingForOtherPlayersOverlayVisible(visible: false);
        RunManager.Instance.OnEnded(isVictory: true);
        // Like the base game: the run ends once every player is "dead" (opens the game over screen as a win)
        foreach (var player in state.Players)
        {
            await CreatureCmd.Kill(player.Creature, force: true);
            await Cmd.CustomScaledWait(0.25f, 0.5f);
        }
    }

    public static void ResetForNewRun() => IsStrifeWin = false;
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.Launch))]
public static class StrifeRunLaunchPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        StrifeVictory.ResetForNewRun();
        StrifeMusic.Silenced = false;
    }
}
