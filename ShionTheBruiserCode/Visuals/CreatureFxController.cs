using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Attaches the combat visual effects to every creature node.
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
public static class CreatureFxPatch
{
    [HarmonyPostfix]
    public static void Postfix(NCreature __instance)
    {
        if (NCombatRoom.Instance == null || __instance.Entity == null || __instance.Visuals == null) return;

        // NCreature._Ready adds its visuals with a deferred call, so they aren't ready yet at this point
        if (__instance.Visuals.IsNodeReady())
        {
            CreatureFxController.Attach(__instance);
            return;
        }
        __instance.Visuals.Connect(
            Node.SignalName.Ready,
            Callable.From(() => CreatureFxController.Attach(__instance)),
            (uint) GodotObject.ConnectFlags.OneShot);
    }
}

/// <summary>
/// Keeps a creature's visual effects in sync with its state:
/// <list type="bullet">
/// <item>Berserk sparks (scenes/fx/berserk_sparks.tscn) on any creature that has the Berserk tracker and is Berserk.</item>
/// <item>Shion's auras (scenes/fx/shion_auras.tscn), stacked in order of application. Any player also gets Shion's blue flames
/// (Spirit Break's) from Burlesque Wind-Up until the Vigor it gave is used up.</item>
/// <item>Flashback bulbs (scenes/fx/memory_bulbs.tscn) above the head, one per stack of Flashback, hopping when Flashback plays cards.</item>
/// <item>Spirit Burst's lightning (scenes/fx/spirit_burst_lightning.tscn) across the screen behind any player, while Spirit Burst
/// autoplays their cards, and once for Borrow Technique.</item>
/// <item>Berserk Form: the base game's smoky vignette (Nightmare's) around the screen, only on the screen of the player who has it.</item>
/// <item>Codependency: salmon pink glow and light (scenes/fx/aura_light.tscn).</item>
/// <item>Come What May: rare sweat drops from Shion's head (scenes/fx/tears.tscn), more often with more stacks, only for Shion.</item>
/// <item>Defensive / Hyper Guard: honeycomb barrier in front of the creature (scenes/fx/hex_shell.tscn).</item>
/// <item>Dragonsong: music notes around the creature, one per point of reduction (scenes/fx/music_notes.tscn).</item>
/// <item>Blood: scarlet wisps circling the creature, one per Blood up to 10, then bigger with more Blood (scenes/fx/blood_wisps.tscn).</item>
/// <item>Full Nelson / Frontliner: the player moves behind the held enemy / in front of the team (<see cref="CreaturePositioning"/>).</item>
/// <item>Eleventh Hour / Extreme Speed: 80% slower animations and a black and white body.</item>
/// <item>Lurking Intensity / Flow Charge: red / orange particles (scenes/fx/rising_particles.tscn).</item>
/// <item>Nap / Eepy: Zs flying away (scenes/fx/zzz.tscn) and 20% slower animations, until the Zs are gone (Nap)
/// or until the creature's next turn (Eepy).</item>
/// </list>
/// Visual state only: nothing here touches the game state.
/// </summary>
public class CreatureFxController
{
    private static string FxScene(string name) => Path.Join(MainFile.ResPath, "scenes", "fx", name + ".tscn");

    private static readonly string SparksScenePath = FxScene("berserk_sparks");
    private static readonly string AurasScenePath = FxScene("shion_auras");
    private static readonly string MemoryBulbsScenePath = FxScene("memory_bulbs");
    private static readonly string SpiritBurstLightningScenePath = FxScene("spirit_burst_lightning");

    /// <summary>Light layer only players' bodies are on, so aura lights only light them. Matches shion_auras.gd.</summary>
    public const int AuraLightLayer = 1 << 19;

    public const int AuraCritThreshold = 50;
    public const int AuraVigorThreshold = 20;

    /// <summary>Eleventh Hour / Extreme Speed: 80% slower.</summary>
    private const float TimeStopSpeed = 0.2f;
    /// <summary>Nap / Eepy: 20% slower.</summary>
    private const float SleepySpeed = 0.8f;
    private const int NapZCount = 4;

    private static readonly Color DefensiveColor = new(0.88f, 0.85f, 1f);
    private static readonly Color HyperGuardColor = new(0.2f, 0.65f, 1f);
    private static readonly Color LurkingIntensityColor = new(1f, 0.15f, 0.15f);
    private static readonly Color FlowChargeColor = new(1f, 0.55f, 0.1f);

    private const float VignetteAlpha = 0.66f;
    private const float VignetteFade = 0.4f;

    /// <summary>Aura names (their spritesheet names) and when each one is active.</summary>
    private static readonly (string Name, Func<Creature, bool> IsActive)[] AuraConditions =
    [
        ("spirit_burst", c => c.HasPower<SpiritBurst>()),
        ("spirit_break", c => c.HasPower<SpiritBreak>()),
        ("dark_arts", c => c.HasPower<DarkArts>()),
        ("fell_dragon", c => c.HasPower<FellDragon>()),
        ("crit_above_50_or_vigor_above_20", c =>
            (c.GetPower<ShionCritTrackerPower>()?.HitRatePercent ?? 0) >= AuraCritThreshold
            || c.GetPowerAmount<VigorPower>() >= AuraVigorThreshold),
    ];

    private readonly NCreature _node;
    private readonly Creature _creature;
    private readonly NCreatureVisuals _visuals;
    private readonly Rect2 _bounds;
    private readonly Node2D _sparks;
    private readonly Node2D? _auras;
    private readonly Node2D? _memoryBulbs;
    private readonly Node2D? _spiritBurstLightning;
    /// <summary>Full Nelson / Frontliner positions (players only).</summary>
    private readonly CreaturePositioning? _positioning;

    // Created the first time they're needed
    private Control? _berserkVignette;
    private Tween? _vignetteTween;
    private bool _vignetteActive;
    private Node2D? _codependencyLight;
    private Node2D? _tears;
    private Node2D? _defensiveShell;
    private Node2D? _hyperGuardShell;
    private Node2D? _notes;
    private Node2D? _bloodWisps;
    private Node2D? _lurkingParticles;
    private Node2D? _flowChargeParticles;
    private Node2D? _zzz;

    private bool _timeStopped;
    /// <summary>Round in which Eepy was played (its Zs stop at the creature's next turn), or null.</summary>
    private int? _eepyRound;

    /// <summary>Burlesque Wind-Up's aura (drawn with Spirit Break's spritesheet, see shion_auras.gd).</summary>
    private const string BurlesqueAura = "burlesque_wind_up";

    /// <summary>
    /// Burlesque Wind-Up's aura stays after the power is removed, until the Vigor gained meanwhile is used up.
    /// Visual state only.
    /// </summary>
    private bool _burlesqueAuraActive;

    /// <summary>Active auras, in order of application.</summary>
    private readonly List<string> _activeAuras = [];

    private CreatureFxController(NCreature node)
    {
        _node = node;
        _creature = node.Entity;
        _visuals = node.Visuals;
        var visuals = node.Visuals;
        _bounds = new Rect2(visuals.Bounds.Position, visuals.Bounds.Size);
        var bounds = _bounds;

        _sparks = GD.Load<PackedScene>(SparksScenePath).Instantiate<Node2D>();
        visuals.AddChild(_sparks);
        _sparks.Call("set_area", bounds.Grow(20f));

        // Only players can get Flashback
        if (_creature.IsPlayer)
        {
            _memoryBulbs = GD.Load<PackedScene>(MemoryBulbsScenePath).Instantiate<Node2D>();
            visuals.AddChild(_memoryBulbs);
            _memoryBulbs.Call("set_area", bounds);
        }

        if (_creature.IsPlayer) _positioning = new CreaturePositioning(node);

        // Shion's auras, and Burlesque Wind-Up's for any player
        if (_creature.IsPlayer)
        {
            _auras = GD.Load<PackedScene>(AurasScenePath).Instantiate<Node2D>();
            visuals.AddChild(_auras);
            // Behind Shion, at her feet
            visuals.MoveChild(_auras, 0);
            _auras.Position = new Vector2(bounds.GetCenter().X, bounds.End.Y);

            if (visuals.GetCurrentBody() is CanvasItem body)
            {
                body.LightMask |= AuraLightLayer;
            }

            // Behind Shion and her auras, at her body's center
            _spiritBurstLightning = GD.Load<PackedScene>(SpiritBurstLightningScenePath).Instantiate<Node2D>();
            visuals.AddChild(_spiritBurstLightning);
            visuals.MoveChild(_spiritBurstLightning, 0);
            _spiritBurstLightning.Position = bounds.GetCenter();
        }
    }

    public static void Attach(NCreature node)
    {
        var controller = new CreatureFxController(node);
        CompanionVisuals.AttachToCombat(node);

        void OnCombatStateChanged(CombatState _) => controller.Refresh();
        void OnFlashbackCardPlayed(Creature creature, int index) => controller.HopMemoryBulb(creature, index);
        void OnSpiritBurstAutoPlayChanged(Creature creature, bool autoPlaying) => controller.SetSpiritBurstLightning(creature, autoPlaying);
        void OnSpiritBurstLightningOnce(Creature creature) => controller.PlaySpiritBurstLightningOnce(creature);
        void OnNapPlayed(Creature creature) => controller.PlayNap(creature);
        void OnEepyPlayed(Creature creature) => controller.PlayEepy(creature);
        void OnDragonsongReduced(Creature creature) => controller.FlashDragonsong(creature);

        CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
        SlalMechanicsHelper.FlashbackCardPlayed += OnFlashbackCardPlayed;
        ShionVfx.SpiritBurstAutoPlayChanged += OnSpiritBurstAutoPlayChanged;
        ShionVfx.SpiritBurstLightningOnce += OnSpiritBurstLightningOnce;
        ShionVfx.NapPlayed += OnNapPlayed;
        ShionVfx.EepyPlayed += OnEepyPlayed;
        ShionVfx.DragonsongReduced += OnDragonsongReduced;
        node.TreeExiting += () =>
        {
            CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
            SlalMechanicsHelper.FlashbackCardPlayed -= OnFlashbackCardPlayed;
            ShionVfx.SpiritBurstAutoPlayChanged -= OnSpiritBurstAutoPlayChanged;
            ShionVfx.SpiritBurstLightningOnce -= OnSpiritBurstLightningOnce;
            ShionVfx.NapPlayed -= OnNapPlayed;
            ShionVfx.EepyPlayed -= OnEepyPlayed;
            ShionVfx.DragonsongReduced -= OnDragonsongReduced;
            // The vignette lives on the run's UI, not under the creature
            if (controller._berserkVignette != null && GodotObject.IsInstanceValid(controller._berserkVignette))
            {
                controller._berserkVignette.QueueFree();
            }
        };
        controller.Refresh();
    }

    private bool IsShion => _creature.Player?.Character is Character.ShionTheBruiser;

    private void Refresh()
    {
        if (!GodotObject.IsInstanceValid(_sparks)) return;

        var alive = _creature.IsAlive;
        _sparks.Call("set_active", alive && _creature.HasPower<BerserkIndicator>() && ShionMechanicsHelper.IsBerserk(_creature));

        if (_memoryBulbs != null && GodotObject.IsInstanceValid(_memoryBulbs))
        {
            _memoryBulbs.Call("set_count", alive ? SlalMechanicsHelper.GetFlashback(_creature) : 0);
        }

        RefreshPowerEffects(alive);
        _positioning?.Refresh();

        if (_auras == null || !GodotObject.IsInstanceValid(_auras)) return;

        var isShion = IsShion;
        foreach (var (name, isActive) in AuraConditions)
        {
            SetAuraActive(name, alive && isShion && isActive(_creature));
        }

        if (_creature.HasPower<BurlesqueWindUp>()) _burlesqueAuraActive = true;
        else if (_creature.GetPowerAmount<VigorPower>() <= 0) _burlesqueAuraActive = false;
        SetAuraActive(BurlesqueAura, alive && _burlesqueAuraActive);

        _auras.Call("set_auras", _activeAuras.ToArray());
    }

    private void RefreshPowerEffects(bool alive)
    {
        var facing = _creature.IsPlayer ? 1f : -1f;

        // Berserk Form: only on the screen of the player who has it
        SetBerserkVignette(alive && _creature.HasPower<BerserkForm>() && _creature.Player != null && LocalContext.IsMe(_creature.Player));

        SetEffect(ref _codependencyLight, "aura_light", alive && _creature.HasPower<Codependency>(),
            n => n.Call("set_area", _bounds), behind: true);

        SetEffect(ref _tears, "tears", alive && IsShion && _creature.HasPower<ComeWhatMay>(), n =>
        {
            var head = ShionVfx.HeadTop(_bounds, true);
            var eyeY = _bounds.Position.Y + _bounds.Size.Y * 0.2f;
            var eyeGap = _bounds.Size.X * 0.12f;
            n.Call("set_head", new Vector2(head.X - eyeGap, eyeY), new Vector2(head.X + eyeGap, eyeY));
        });
        _tears?.Call("set_stacks", _creature.GetPowerAmount<ComeWhatMay>());

        SetEffect(ref _defensiveShell, "hex_shell", alive && _creature.HasPower<DefensiveDebuff>(), n =>
        {
            n.Call("set_style", DefensiveColor, 2.5f, 1);
            n.Call("set_area", _bounds, facing);
        });
        SetEffect(ref _hyperGuardShell, "hex_shell", alive && _creature.HasPower<HyperGuard>(), n =>
        {
            n.Call("set_style", HyperGuardColor, 5f, 2);
            n.Call("set_area", _bounds.Grow(12f), facing);
        });

        var dragonsong = _creature.GetPower<Dragonsong>();
        var noteCount = alive && dragonsong != null
            ? Math.Min(ShionMechanicsHelper.GetDragonUniqueBuffCount(_creature), dragonsong.Amount)
            : 0;
        SetEffect(ref _notes, "music_notes", noteCount > 0, n => n.Call("set_area", _bounds));
        _notes?.Call("set_count", noteCount);

        var blood = alive ? SeelsMechanicsHelper.GetBlood(_creature) : 0;
        SetEffect(ref _bloodWisps, "blood_wisps", blood > 0, n => n.Call("set_area", _bounds));
        _bloodWisps?.Call("set_blood", blood);

        SetEffect(ref _lurkingParticles, "rising_particles", alive && _creature.HasPower<LurkingIntensity>(), n =>
        {
            n.Call("set_color", LurkingIntensityColor);
            n.Call("set_area", _bounds);
        });
        SetEffect(ref _flowChargeParticles, "rising_particles", alive && _creature.HasPower<FlowCharge>(), n =>
        {
            n.Call("set_color", FlowChargeColor);
            n.Call("set_area", _bounds);
        });

        // Eleventh Hour (on Shion) / Extreme Speed (on enemies): slowed down, black and white
        var timeStopped = alive && (_creature.HasPower<EleventhHour>() || _creature.HasPower<ExtremeSpeed>());
        if (timeStopped != _timeStopped)
        {
            _timeStopped = timeStopped;
            CreatureLook.SetSpeed(_node, "time_stop", timeStopped ? TimeStopSpeed : null);
            CreatureLook.SetMaterial(_node, "time_stop", CreatureLook.GrayscalePriority, timeStopped ? CreatureLook.Grayscale : null);
        }

        // Eepy lasts until the creature's next turn
        if (_eepyRound is { } round && _creature.CombatState is { } state
            && (!alive || (state.RoundNumber > round && state.CurrentSide == _creature.Side)))
        {
            _eepyRound = null;
            _zzz?.Call("set_active", false);
            CreatureLook.SetSpeed(_node, "eepy", null);
        }
    }

    /// <summary>
    /// Creates the effect the first time it turns on (with <paramref name="setup"/>), then toggles it with set_active().
    /// </summary>
    private void SetEffect(ref Node2D? node, string scene, bool active, Action<Node2D> setup, bool behind = false)
    {
        if (node == null)
        {
            if (!active || !GodotObject.IsInstanceValid(_visuals)) return;
            node = GD.Load<PackedScene>(FxScene(scene)).Instantiate<Node2D>();
            _visuals.AddChild(node);
            if (behind) _visuals.MoveChild(node, 0);
            setup(node);
        }
        if (!GodotObject.IsInstanceValid(node)) return;
        if (node.HasMethod("set_active")) node.Call("set_active", active);
    }

    private void SetBerserkVignette(bool active)
    {
        if (active == _vignetteActive) return;
        _vignetteActive = active;

        if (active && (_berserkVignette == null || !GodotObject.IsInstanceValid(_berserkVignette)))
        {
            _berserkVignette = ShionVfx.CreateBerserkVignette();
            if (_berserkVignette == null) return;
            ShionVfx.AddToScreen(_berserkVignette);
        }
        if (_berserkVignette == null || !GodotObject.IsInstanceValid(_berserkVignette)) return;

        _vignetteTween?.Kill();
        _vignetteTween = _berserkVignette.CreateTween();
        _vignetteTween.TweenProperty(_berserkVignette, "modulate:a", active ? VignetteAlpha : 0f, VignetteFade);
    }

    private void SetAuraActive(string name, bool active)
    {
        if (active && !_activeAuras.Contains(name)) _activeAuras.Add(name);
        else if (!active) _activeAuras.Remove(name);
    }

    private void SetSpiritBurstLightning(Creature creature, bool active)
    {
        if (creature != _creature || _spiritBurstLightning == null || !GodotObject.IsInstanceValid(_spiritBurstLightning)) return;
        _spiritBurstLightning.Call("set_active", active && _creature.IsAlive);
    }

    private void PlaySpiritBurstLightningOnce(Creature creature)
    {
        if (creature != _creature || _spiritBurstLightning == null || !GodotObject.IsInstanceValid(_spiritBurstLightning)) return;
        _spiritBurstLightning.Call("play_once");
    }

    private void HopMemoryBulb(Creature creature, int index)
    {
        if (creature != _creature || _memoryBulbs == null || !GodotObject.IsInstanceValid(_memoryBulbs)) return;
        _memoryBulbs.Call("hop", index);
    }

    private void FlashDragonsong(Creature creature)
    {
        if (creature != _creature || _notes == null || !GodotObject.IsInstanceValid(_notes)) return;
        _notes.Call("flash");
    }

    private Node2D? EnsureZzz()
    {
        if (_zzz != null && GodotObject.IsInstanceValid(_zzz)) return _zzz;
        if (!GodotObject.IsInstanceValid(_visuals)) return null;
        _zzz = GD.Load<PackedScene>(FxScene("zzz")).Instantiate<Node2D>();
        _visuals.AddChild(_zzz);
        _zzz.Call("set_origin", ShionVfx.HeadTop(_bounds, _creature.IsPlayer) + new Vector2(_bounds.Size.X * 0.15f, 0f));
        // Nap's slowdown lasts until its Zs are gone
        _zzz.Connect("cleared", Callable.From(() => CreatureLook.SetSpeed(_node, "nap", null)));
        return _zzz;
    }

    private void PlayNap(Creature creature)
    {
        if (creature != _creature || EnsureZzz() is not { } zzz) return;
        zzz.Call("play_burst", NapZCount);
        CreatureLook.SetSpeed(_node, "nap", SleepySpeed);
    }

    private void PlayEepy(Creature creature)
    {
        if (creature != _creature || EnsureZzz() is not { } zzz || _creature.CombatState == null) return;
        _eepyRound = _creature.CombatState.RoundNumber;
        zzz.Call("set_active", true);
        CreatureLook.SetSpeed(_node, "eepy", SleepySpeed);
    }
}
