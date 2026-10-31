using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
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
/// </list>
/// </summary>
public class CreatureFxController
{
    private static readonly string SparksScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "berserk_sparks.tscn");
    private static readonly string AurasScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "shion_auras.tscn");
    private static readonly string MemoryBulbsScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "memory_bulbs.tscn");

    /// <summary>Light layer only players' bodies are on, so aura lights only light them. Matches shion_auras.gd.</summary>
    public const int AuraLightLayer = 1 << 19;

    public const int AuraCritThreshold = 50;
    public const int AuraVigorThreshold = 20;

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

    private readonly Creature _creature;
    private readonly Node2D _sparks;
    private readonly Node2D? _auras;
    private readonly Node2D? _memoryBulbs;

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
        _creature = node.Entity;
        var visuals = node.Visuals;
        var bounds = new Rect2(visuals.Bounds.Position, visuals.Bounds.Size);

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
        }
    }

    public static void Attach(NCreature node)
    {
        var controller = new CreatureFxController(node);

        void OnCombatStateChanged(CombatState _) => controller.Refresh();
        void OnFlashbackCardPlayed(Creature creature, int index) => controller.HopMemoryBulb(creature, index);

        CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
        SlalMechanicsHelper.FlashbackCardPlayed += OnFlashbackCardPlayed;
        node.TreeExiting += () =>
        {
            CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
            SlalMechanicsHelper.FlashbackCardPlayed -= OnFlashbackCardPlayed;
        };
        controller.Refresh();
    }

    private void Refresh()
    {
        if (!GodotObject.IsInstanceValid(_sparks)) return;

        var alive = _creature.IsAlive;
        _sparks.Call("set_active", alive && _creature.HasPower<BerserkIndicator>() && ShionMechanicsHelper.IsBerserk(_creature));

        if (_memoryBulbs != null && GodotObject.IsInstanceValid(_memoryBulbs))
        {
            _memoryBulbs.Call("set_count", alive ? SlalMechanicsHelper.GetFlashback(_creature) : 0);
        }

        if (_auras == null || !GodotObject.IsInstanceValid(_auras)) return;

        var isShion = _creature.Player?.Character is Character.ShionTheBruiser;
        foreach (var (name, isActive) in AuraConditions)
        {
            SetAuraActive(name, alive && isShion && isActive(_creature));
        }

        if (_creature.HasPower<BurlesqueWindUp>()) _burlesqueAuraActive = true;
        else if (_creature.GetPowerAmount<VigorPower>() <= 0) _burlesqueAuraActive = false;
        SetAuraActive(BurlesqueAura, alive && _burlesqueAuraActive);

        _auras.Call("set_auras", _activeAuras.ToArray());
    }

    private void SetAuraActive(string name, bool active)
    {
        if (active && !_activeAuras.Contains(name)) _activeAuras.Add(name);
        else if (!active) _activeAuras.Remove(name);
    }

    private void HopMemoryBulb(Creature creature, int index)
    {
        if (creature != _creature || _memoryBulbs == null || !GodotObject.IsInstanceValid(_memoryBulbs)) return;
        _memoryBulbs.Call("hop", index);
    }
}
