using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Slots and positions of the creatures the Act 4 Architect summons.
/// <list type="bullet">
/// <item>Game state: the slot names of the encounters he summoned this fight, so their monsters (and the creatures they
/// spawn, like Phrog Parasite's Wrigglers) find their slots in <see cref="StrifeArchitectEncounter.Slots"/>. Set from his
/// summon move, so it's the same on every client.</item>
/// <item>Visual only: each summoned encounter keeps its own layout (its scene's slot markers, or the game's default row
/// for encounters without a scene). When one summon line has two encounters, the one with the most creatures stands back
/// (up, to the right, a bit smaller) and the other one a step forward (down, to the left, drawn on top). The Architect leans back
/// while he has summons.</item>
/// </list>
/// </summary>
public static class StrifeLayout
{
    private static readonly List<string> Slots = [];

    public static IReadOnlyList<string> SummonedSlots => Slots;

    /// <summary>One encounter he summoned: its layout, and its depth in its summon line (0 = background).</summary>
    private sealed class Group(EncounterModel encounter, int indexInLine, int lineSize)
    {
        public EncounterModel Encounter { get; } = encounter;
        public int IndexInLine { get; } = indexInLine;
        public int LineSize { get; } = lineSize;
        private Dictionary<string, Vector2>? _markers;

        public bool IsBack => LineSize > 1 && IndexInLine == 0;
        public bool IsFront => LineSize > 1 && IndexInLine > 0;

        /// <summary>Its scene's markers, in enemy container coordinates (like the game's encounter scenes).</summary>
        public Dictionary<string, Vector2> Markers => _markers ??= LoadMarkers(Encounter);
    }

    private static readonly List<Group> Groups = [];
    private static Group? _current;
    private static readonly ConditionalWeakTable<Creature, Group> Membership = new();

    /// <summary>Start of the fight.</summary>
    public static void Reset()
    {
        Slots.Clear();
        Groups.Clear();
        _current = null;
    }

    /// <summary>Called by the Architect before he adds an encounter's monsters.</summary>
    public static void BeginGroup(EncounterModel encounter, int indexInLine, int lineSize)
    {
        foreach (var slot in encounter.Slots.Concat(encounter.MonstersWithSlots.Select(m => m.Item2)))
        {
            if (!string.IsNullOrEmpty(slot) && !Slots.Contains(slot)) Slots.Add(slot);
        }
        _current = new Group(encounter, indexInLine, lineSize);
        Groups.Add(_current);
    }

    public static void EndGroup() => _current = null;

    /// <summary>
    /// Which summoned encounter a creature belongs to: the one being summoned, else the one whose slots it uses
    /// (Wrigglers...), else the latest one.
    /// </summary>
    private static void Assign(Creature creature)
    {
        if (Membership.TryGetValue(creature, out _)) return;
        var group = _current
                    ?? Groups.LastOrDefault(g => creature.SlotName != null && (g.Encounter.Slots.Contains(creature.SlotName)))
                    ?? Groups.LastOrDefault();
        if (group != null) Membership.AddOrUpdate(creature, group);
    }

    // ── Visuals ──────────────────────────────────────────────────────────────

    /// <summary>The game's encounter scenes are 1920x1080 and moved by half that into the enemy container.</summary>
    private static readonly Vector2 SceneOrigin = new(960f, 540f);

    /// <summary>The game's default enemy row (NCombatRoom.PositionEnemies).</summary>
    private const float RowWidth = 960f;
    private const float RowGap = 70f;
    private const float RowLeft = 150f;
    private const float Baseline = 200f;

    private static readonly Vector2 BackOffset = new(110f, -70f);
    private const float BackScale = 0.9f;
    private static readonly Vector2 FrontOffset = new(-170f, 100f);

    private const float LeanScale = 0.78f;
    private static readonly Vector2 LeanOffset = new(160f, -150f);
    private const float MoveTime = 0.35f;
    private const string HomeMeta = "strife_home";

    private static readonly Func<NCombatRoom, Control?> EncounterSlotsGetter =
        AccessTools.MethodDelegate<Func<NCombatRoom, Control?>>(AccessTools.PropertyGetter(typeof(NCombatRoom), "EncounterSlots"));

    public static bool IsStrifeFight(Creature? creature) => creature?.CombatState?.Encounter is StrifeArchitectEncounter;

    /// <summary>Makes sure the room's slot node has a marker for that slot (they're only used as a starting point).</summary>
    public static void EnsureMarker(NCombatRoom room, Creature creature)
    {
        Assign(creature);
        var slot = creature.SlotName;
        if (string.IsNullOrEmpty(slot)) return;
        var slots = EncounterSlotsGetter(room);
        if (slots == null || slots.HasNode(slot)) return;
        slots.AddChild(new Marker2D { Name = slot, Position = new Vector2(1500f, 740f) });
    }

    private static Dictionary<string, Vector2> LoadMarkers(EncounterModel encounter)
    {
        var markers = new Dictionary<string, Vector2>();
        if (!encounter.HasScene) return markers;
        var path = SceneHelper.GetScenePath("encounters/" + encounter.Id.Entry.ToLowerInvariant());
        if (!ResourceLoader.Exists(path)) return markers;
        var scene = ResourceLoader.Load<PackedScene>(path).Instantiate<Control>();
        foreach (var marker in scene.GetChildren().OfType<Marker2D>())
        {
            markers[marker.Name.ToString()] = marker.Position - SceneOrigin;
        }
        scene.Free();
        return markers;
    }

    public static void Arrange(NCombatRoom room)
    {
        var architect = room.CreatureNodes.FirstOrDefault(n =>
            GodotObject.IsInstanceValid(n) && n.Entity?.Monster is StrifeArchitect);
        var summons = room.CreatureNodes
            .Where(n => GodotObject.IsInstanceValid(n) && n.Entity is { Side: CombatSide.Enemy } e && e.Monster is not StrifeArchitect && e.IsAlive)
            .ToList();

        if (architect != null)
        {
            var home = HomeOf(architect);
            var leaning = summons.Count > 0;
            architect.ZIndex = 0;
            Move(architect, leaning ? home + LeanOffset : home, Vector2.One * (leaning ? LeanScale : 1f));
            // Drawn first among the enemies: behind his summons
            architect.GetParent()?.MoveChild(architect, 0);
        }
        if (summons.Count == 0) return;

        var roomScale = room.CreatureNodes.FirstOrDefault()?.Entity?.CombatState?.Encounter?.GetCameraScaling() ?? 1f;
        var drawOrder = new List<(NCreature Node, int Depth, float X)>();
        foreach (var members in summons.GroupBy(n => Membership.TryGetValue(n.Entity, out var g) ? g : null))
        {
            drawOrder.AddRange(ArrangeGroup(members.Key, members.ToList(), roomScale));
        }

        // Draw order by sibling order, not ZIndex: ZIndex also put them above the pause menu. Background encounter
        // first, foreground last; in each, the leftmost last (on top).
        foreach (var (node, _, _) in drawOrder.OrderBy(e => e.Depth).ThenByDescending(e => e.X))
        {
            if (node.GetParent() is { } parent) parent.MoveChild(node, parent.GetChildCount() - 1);
        }
    }

    /// <returns>Each creature with its depth (0 back, 1 alone, 2 front) and its target x, for the draw order.</returns>
    private static List<(NCreature, int, float)> ArrangeGroup(Group? group, List<NCreature> members, float roomScale)
    {
        // The encounter's own camera zoom (bosses are often zoomed out), relative to this room's
        var zoom = group == null ? 1f : group.Encounter.GetCameraScaling() / roomScale;
        var depth = group?.IsBack == true ? BackScale : 1f;
        var scale = zoom * depth;
        var offset = group?.IsBack == true ? BackOffset : group?.IsFront == true ? FrontOffset : Vector2.Zero;

        // Slotted creatures go to their marker; the others get the game's default row, among themselves
        var positions = new Dictionary<NCreature, Vector2>();
        var unslotted = new List<NCreature>();
        foreach (var node in members)
        {
            var slot = node.Entity.SlotName;
            if (group != null && !string.IsNullOrEmpty(slot) && group.Markers.TryGetValue(slot, out var marker)) positions[node] = marker;
            else unslotted.Add(node);
        }
        foreach (var (node, position) in DefaultRow(unslotted, group?.Encounter.GetCameraScaling() ?? 1f)) positions[node] = position;

        var depthRank = group?.IsFront == true ? 2 : group?.IsBack == true ? 0 : 1;
        var result = new List<(NCreature, int, float)>();
        foreach (var (node, position) in positions)
        {
            var target = position * scale + offset;
            node.ZIndex = 0;
            Move(node, target, Vector2.One * scale);
            result.Add((node, depthRank, target.X));
        }
        return result;
    }

    /// <summary>Same maths as the game's NCombatRoom.PositionEnemies.</summary>
    private static IEnumerable<(NCreature, Vector2)> DefaultRow(List<NCreature> creatures, float scaling)
    {
        if (creatures.Count == 0) yield break;
        var available = RowWidth / scaling;
        var gap = RowGap;
        var widths = creatures.Sum(n => n.Visuals.Bounds.Size.X);
        var used = widths + (creatures.Count - 1) * gap;
        var x = Math.Max((available - used) * 0.5f, RowLeft);
        var stagger = 0f;
        if (x + used > available && creatures.Count > 1)
        {
            gap = Math.Max((available - RowLeft - widths) / (creatures.Count - 1), 5f);
            used = widths + (creatures.Count - 1) * gap;
            x = (available - used) * 0.5f;
            if (gap < 30f) stagger = float.Lerp(60f, 40f, (gap - 5f) / 25f);
        }
        for (var i = 0; i < creatures.Count; i++)
        {
            var width = creatures[i].Visuals.Bounds.Size.X;
            yield return (creatures[i], new Vector2(x + width * 0.5f, Baseline - (i % 2 != 0 ? stagger : 0f)));
            x += width + gap;
        }
    }

    /// <summary>His position at his marker, remembered the first time he's moved.</summary>
    private static Vector2 HomeOf(NCreature architect)
    {
        if (architect.HasMeta(HomeMeta)) return architect.GetMeta(HomeMeta).AsVector2();
        architect.SetMeta(HomeMeta, architect.Position);
        return architect.Position;
    }

    private static void Move(NCreature node, Vector2 position, Vector2 scale)
    {
        var tween = node.CreateTween().SetParallel();
        tween.TweenProperty(node, "position", position, MoveTime).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(node, "scale", scale, MoveTime).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
    }

    /// <summary>Lays the room out again on the next frame (after a summon is added or dies).</summary>
    public static void ArrangeDeferred()
    {
        if (NCombatRoom.Instance is { } room) Callable.From(() => { if (GodotObject.IsInstanceValid(room)) Arrange(room); }).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature))]
public static class StrifeAddCreaturePatch
{
    [HarmonyPrefix]
    public static void Prefix(NCombatRoom __instance, Creature creature)
    {
        if (!StrifeLayout.IsStrifeFight(creature) || creature.Monster is StrifeArchitect) return;
        StrifeLayout.EnsureMarker(__instance, creature);
    }

    [HarmonyPostfix]
    public static void Postfix(NCombatRoom __instance, Creature creature)
    {
        if (!StrifeLayout.IsStrifeFight(creature) || creature.Monster is StrifeArchitect || creature.Side != CombatSide.Enemy) return;
        // Wait a frame: the new node's visuals (and their bounds) are added deferred
        StrifeLayout.ArrangeDeferred();
    }
}
