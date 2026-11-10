using BaseLib.Audio;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Act 4's music, played through BaseLib's ModAudio (looping) instead of the game's FMOD act music:
/// <list type="bullet">
/// <item>"All Clear" from the Chaos Emeralds cutscene on, and everywhere in Act 4 outside of fights.</item>
/// <item>Each track loops between its own points (<see cref="LoopPoints"/>).</item>
/// <item>In a fight with Super Form: "Egg Reverie" while the local player has Rings, "Egg Panicky" at 0 Rings.</item>
/// </list>
/// Visual/audio only: never touches the game state.
/// </summary>
public static class StrifeMusic
{
    private static string Audio(string folder, string file) => Path.Join(MainFile.ResPath, "audio", folder, file);

    public static readonly string AllClear = Audio("music", "all_clear.mp3");
    public static readonly string EggReverie = Audio("music", "egg_reverie.mp3");
    public static readonly string EggPanic = Audio("music", "egg_panic.mp3");
    public static readonly string TransformSfx = Audio("sfx", "s3k_6a.wav");

    /// <summary>Custom loop points, in seconds: when a track reaches LoopEnd, it jumps back to LoopStart.</summary>
    private static readonly Dictionary<string, (double LoopStart, double LoopEnd)> LoopPoints = new()
    {
        [AllClear] = (0.0, 53.0),
        [EggReverie] = (3.0, 2 * 60 + 52.0),
        [EggPanic] = (1.0, 60 + 14.0),
    };

    private static string? _current;
    private static AudioStreamPlayer? _player;
    private static bool _loopWatcherConnected;

    /// <summary>Set during the end-of-Act-3 cutscene, before Act 4 is part of the run.</summary>
    public static bool CutsceneActive { get; set; }

    private static bool IsPlaying => _player != null && GodotObject.IsInstanceValid(_player) && _player.IsInsideTree();

    /// <summary>Set once the Act 4 Architect is "defeated": no more music for the rest of the run.</summary>
    public static bool Silenced { get; set; }

    /// <summary>Picks the track for the current situation, and switches to it if needed.</summary>
    public static void Refresh()
    {
        if (Silenced)
        {
            Stop();
            return;
        }
        var state = StrifeRun.State;
        if (!CutsceneActive && !StrifeAct.IsStrife(state))
        {
            if (_current != null) Stop();
            return;
        }

        var track = AllClear;
        var me = state == null ? null : LocalContext.GetMe(state.Players);
        if (!CutsceneActive && me != null && CombatManager.Instance.IsInProgress && me.Creature.HasPower<SuperFormPower>())
        {
            track = me.Gold > 0 ? EggReverie : EggPanic;
        }
        Play(track);
    }

    private static void Play(string path)
    {
        if (_current == path && IsPlaying) return;
        if (!ResourceLoader.Exists(path)) return;

        NRunMusicController.Instance?.StopMusic();
        // Removed first: BaseLib's ModAudio throws ("Collection was modified") when it replaces a playing music track
        // itself, which left the music silent until the next switch
        Stop();
        var sound = new ModSound(path, ModAudio.SoundType.Music);
        var loopStart = LoopPoints.TryGetValue(path, out var points) ? points.LoopStart : 0.0;
        // Looping at the end of the file is only a fallback: WatchLoop jumps back at LoopEnd
        switch (sound.GetOrLoadStream())
        {
            case AudioStreamMP3 mp3:
                mp3.Loop = true;
                mp3.LoopOffset = loopStart;
                break;
            case AudioStreamOggVorbis ogg:
                ogg.Loop = true;
                ogg.LoopOffset = loopStart;
                break;
        }
        _player = ModAudio.PlaySoundInRun(sound);
        _current = path;
        ConnectLoopWatcher();
    }

    private static void ConnectLoopWatcher()
    {
        if (_loopWatcherConnected || Engine.GetMainLoop() is not SceneTree tree) return;
        tree.ProcessFrame += WatchLoop;
        _loopWatcherConnected = true;
    }

    /// <summary>Every frame: once the current track passes its LoopEnd, seek back to its LoopStart.</summary>
    private static void WatchLoop()
    {
        if (_current == null || !IsPlaying || !_player!.Playing) return;
        if (!LoopPoints.TryGetValue(_current, out var points)) return;
        var position = _player.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix();
        if (position >= points.LoopEnd) _player.Seek((float)(points.LoopStart + (position - points.LoopEnd)));
    }

    public static void Stop()
    {
        if (IsPlaying)
        {
            _player!.Stop();
            _player.GetParent()?.RemoveChild(_player);
        }
        _player = null;
        _current = null;
    }

    public static void PlayTransformSfx()
    {
        if (ResourceLoader.Exists(TransformSfx)) new ModSound(TransformSfx).Play();
    }
}

/// <summary>In Act 4, the game's own act/room music stays off: <see cref="StrifeMusic"/> plays instead.</summary>
[HarmonyPatch(typeof(NRunMusicController))]
public static class StrifeMusicControllerPatches
{
    private static bool ShouldSkip()
    {
        if (!StrifeMusic.CutsceneActive && !StrifeAct.IsStrife(StrifeRun.State)) return false;
        StrifeMusic.Refresh();
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.UpdateMusic))]
    public static bool UpdateMusic() => !ShouldSkip();

    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.UpdateTrack), new Type[] { })]
    public static bool UpdateTrack() => !ShouldSkip();

    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.UpdateAmbience))]
    public static bool UpdateAmbience() => !ShouldSkip();

    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.ToggleMerchantTrack))]
    public static bool ToggleMerchantTrack() => !ShouldSkip();

    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.PlayCustomMusic))]
    public static bool PlayCustomMusic() => !ShouldSkip();

    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.UpdateCustomTrack))]
    public static bool UpdateCustomTrack() => !ShouldSkip();

    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.UpdateMusicParameter))]
    public static bool UpdateMusicParameter() => !ShouldSkip();

    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.TriggerEliteSecondPhase))]
    public static bool TriggerEliteSecondPhase() => !ShouldSkip();

    // After a rest site option, the campfire's FMOD ambience "goes out". In Act 4 it was never started and its act bank
    // is unloaded (StopMusic), and calling it crashed the game natively (after a Smith).
    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.TriggerCampfireGoingOut))]
    public static bool TriggerCampfireGoingOut() => !ShouldSkip();

    [HarmonyPrefix, HarmonyPatch(nameof(NRunMusicController.StopCustomMusic))]
    public static bool StopCustomMusic() => !ShouldSkip();
}
