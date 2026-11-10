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

    private static string? _current;
    private static AudioStreamPlayer? _player;

    /// <summary>Set during the end-of-Act-3 cutscene, before Act 4 is part of the run.</summary>
    public static bool CutsceneActive { get; set; }

    private static bool IsPlaying => _player != null && GodotObject.IsInstanceValid(_player) && _player.IsInsideTree();

    /// <summary>Picks the track for the current situation, and switches to it if needed.</summary>
    public static void Refresh()
    {
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
        var sound = new ModSound(path, ModAudio.SoundType.Music);
        switch (sound.GetOrLoadStream())
        {
            case AudioStreamMP3 mp3:
                mp3.Loop = true;
                break;
            case AudioStreamOggVorbis ogg:
                ogg.Loop = true;
                break;
        }
        _player = ModAudio.PlaySoundInRun(sound);
        _current = path;
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
}
