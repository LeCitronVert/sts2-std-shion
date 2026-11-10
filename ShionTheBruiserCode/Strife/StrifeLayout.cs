using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
/// <item>Visual only: the fight's scene only has the Architect's marker, so the summoned slots get a marker the first
/// time a creature uses them, then the enemies are laid out again (<see cref="Arrange"/>), up to 4 front + 4 back spots.</item>
/// </list>
/// </summary>
public static class StrifeLayout
{
    private static readonly List<string> Slots = [];

    public static IReadOnlyList<string> SummonedSlots => Slots;

    /// <summary>Start of the fight.</summary>
    public static void Reset() => Slots.Clear();

    /// <summary>Called by the Architect before he adds an encounter's monsters.</summary>
    public static void BeginGroup(EncounterModel encounter)
    {
        foreach (var slot in encounter.Slots.Concat(encounter.MonstersWithSlots.Select(m => m.Item2)))
        {
            if (!string.IsNullOrEmpty(slot) && !Slots.Contains(slot)) Slots.Add(slot);
        }
    }

    // ── Visuals ──────────────────────────────────────────────────────────────

    /// <summary>Left edge of the summons' area, right of the screen center (same as the game's enemy layout).</summary>
    private const float AreaLeft = 150f;
    private const float AreaRight = 900f;
    private const float Baseline = 200f;
    private const float MoveTime = 0.35f;

    private static readonly Func<NCombatRoom, Control?> EncounterSlotsGetter =
        AccessTools.MethodDelegate<Func<NCombatRoom, Control?>>(AccessTools.PropertyGetter(typeof(NCombatRoom), "EncounterSlots"));

    public static bool IsStrifeFight(Creature? creature) => creature?.CombatState?.Encounter is StrifeArchitectEncounter;

    /// <summary>Makes sure the room's slot node has a marker for that slot (they're only used as a starting point).</summary>
    public static void EnsureMarker(NCombatRoom room, string slot)
    {
        var slots = EncounterSlotsGetter(room);
        if (slots == null || slots.HasNode(slot)) return;
        slots.AddChild(new Marker2D { Name = slot, Position = new Vector2(1500f, 740f) });
    }

    /// <summary>
    /// While he has summons, the Architect leans back (smaller, further up and to the right, drawn behind them) so his
    /// upper body stays free to hover. His summons stand in front of him on two staggered rows of 4 spots: going left to
    /// right, the spots alternate front row / back row (back row higher and a bit smaller, for depth). Front row is drawn
    /// above the back row, and in each row the leftmost summon is drawn on top. Without summons he comes back to his marker.
    /// </summary>
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
            architect.ZIndex = leaning ? -1 : 0;
            Move(architect, leaning ? home + LeanOffset : home, Vector2.One * (leaning ? LeanScale : 1f));
        }
        if (summons.Count == 0) return;

        // Spots spread over the whole area; with few summons they're further apart, centered
        var count = summons.Count;
        var width = AreaRight - AreaLeft;
        var step = width / Math.Max(count, SpotsPerRow);
        var start = AreaLeft + (width - step * (count - 1)) * 0.5f;

        for (var i = 0; i < count; i++)
        {
            var node = summons[i];
            // 1 summon alone stays on the front row; otherwise the spots alternate front / back
            var back = count > 1 && i % 2 == 1;
            var target = new Vector2(start + step * i, back ? Baseline - BackRowRise : Baseline);
            node.ZIndex = (back ? BackRowZ : FrontRowZ) + count - i;
            Move(node, target, Vector2.One * (back ? BackRowScale : 1f));
        }
    }

    private const int SpotsPerRow = 4;
    private const float BackRowRise = 120f;
    private const float BackRowScale = 0.86f;
    private const int FrontRowZ = 20;
    private const int BackRowZ = 1;

    private const float LeanScale = 0.78f;
    private static readonly Vector2 LeanOffset = new(160f, -150f);
    private const string HomeMeta = "strife_home";

    /// <summary>His position at his marker, remembered the first time he's moved.</summary>
    private static Vector2 HomeOf(NCreature architect)
    {
        if (architect.HasMeta(HomeMeta)) return architect.GetMeta(HomeMeta).AsVector2();
        architect.SetMeta(HomeMeta, architect.Position);
        return architect.Position;
    }

    private static void Move(NCreature node, Vector2 position, Vector2? scale)
    {
        var tween = node.CreateTween().SetParallel();
        tween.TweenProperty(node, "position", position, MoveTime).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        if (scale is { } s) tween.TweenProperty(node, "scale", s, MoveTime).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
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
        if (!StrifeLayout.IsStrifeFight(creature) || string.IsNullOrEmpty(creature.SlotName)) return;
        StrifeLayout.EnsureMarker(__instance, creature.SlotName);
    }

    [HarmonyPostfix]
    public static void Postfix(NCombatRoom __instance, Creature creature)
    {
        if (!StrifeLayout.IsStrifeFight(creature) || creature.Monster is StrifeArchitect || creature.Side != CombatSide.Enemy) return;
        // Wait a frame: the new node's visuals (and their bounds) are added deferred
        StrifeLayout.ArrangeDeferred();
    }
}
