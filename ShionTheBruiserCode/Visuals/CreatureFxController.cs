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
/// <item>Shion's auras (scenes/fx/shion_auras.tscn), stacked in order of application.</item>
/// </list>
/// </summary>
public class CreatureFxController
{
    private static readonly string SparksScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "berserk_sparks.tscn");
    private static readonly string AurasScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "shion_auras.tscn");

    /// <summary>Light layer only Shion's body is on, so her aura lights only light her. Matches shion_auras.gd.</summary>
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

        if (_creature.Player?.Character is Character.ShionTheBruiser)
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

        CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
        node.TreeExiting += () => CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
        controller.Refresh();
    }

    private void Refresh()
    {
        if (!GodotObject.IsInstanceValid(_sparks)) return;

        var alive = _creature.IsAlive;
        _sparks.Call("set_active", alive && _creature.HasPower<BerserkIndicator>() && ShionMechanicsHelper.IsBerserk(_creature));

        if (_auras == null || !GodotObject.IsInstanceValid(_auras)) return;

        foreach (var (name, isActive) in AuraConditions)
        {
            var active = alive && isActive(_creature);
            if (active && !_activeAuras.Contains(name)) _activeAuras.Add(name);
            else if (!active) _activeAuras.Remove(name);
        }
        _auras.Call("set_auras", _activeAuras.ToArray());
    }
}
