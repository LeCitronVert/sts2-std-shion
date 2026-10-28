using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

/// <summary>
/// In-mod copy of BaseLib PR #331's NTimelineScreenVerticalDragging: era columns are sized for 5 epochs.
/// When one holds more, the slots are offset and the timeline can also be dragged vertically.
/// Same numbers as the PR. Differences: no transpiler (the vertical delta is added around ProcessPanEvent),
/// and the offset is not stacked again when a timeline expansion adds slots.
///
/// The PR's other layout patch (moving epochs that share an era position) isn't copied:
/// <see cref="EpochValidator"/> reports such collisions instead, pick a free row.
/// </summary>
[HarmonyPatch]
public static class TimelineOverflowPatches
{
    private const int VisibleRows = 5;
    private const float EpochSize = -24f;
    private const float Spacing = -32f;

    private static Vector2 _appliedOffset;
    private static bool _allowVerticalDrag;

    private static readonly AccessTools.FieldRef<NTimelineScreen, HBoxContainer> EpochSlotContainer =
        AccessTools.FieldRefAccess<NTimelineScreen, HBoxContainer>("_epochSlotContainer");
    private static readonly AccessTools.FieldRef<NTimelineScreen, NSlotsContainer> SlotsContainer =
        AccessTools.FieldRefAccess<NTimelineScreen, NSlotsContainer>("_slotsContainer");
    private static readonly AccessTools.FieldRef<NSlotsContainer, Vector2> DragStartPosition =
        AccessTools.FieldRefAccess<NSlotsContainer, Vector2>("_dragStartPosition");
    private static readonly AccessTools.FieldRef<NSlotsContainer, Vector2> TargetPosition =
        AccessTools.FieldRefAccess<NSlotsContainer, Vector2>("_targetPosition");
    private static readonly AccessTools.FieldRef<NSlotsContainer, bool> IsDragging =
        AccessTools.FieldRefAccess<NSlotsContainer, bool>("_isDragging");
    private static readonly AccessTools.FieldRef<NSlotsContainer, Control> EpochSlots =
        AccessTools.FieldRefAccess<NSlotsContainer, Control>("_epochSlots");
    private static readonly AccessTools.FieldRef<NSlotsContainer, Control> WhatsMoved =
        AccessTools.FieldRefAccess<NSlotsContainer, Control>("_whatsMoved");

    private static int TallestColumn(HBoxContainer epochSlotContainer) =>
        epochSlotContainer.GetChildren().OfType<NEraColumn>()
            .Select(column => column.GetChildren().OfType<NEpochSlot>().Count())
            .Prepend(0)
            .Max();

    /// <summary>
    /// Runs once the slots are added (the async part only animates them).
    /// </summary>
    [HarmonyPatch(typeof(NTimelineScreen), nameof(NTimelineScreen.AddEpochSlots))]
    [HarmonyPostfix]
    private static void OffsetTallColumns(NTimelineScreen __instance)
    {
        var epochSlotContainer = EpochSlotContainer(__instance);
        var overCount = TallestColumn(epochSlotContainer) - VisibleRows;
        var offset = overCount <= 0 ? Vector2.Zero : new Vector2(0, overCount * EpochSize + (overCount - 1) * Spacing);
        epochSlotContainer.Position += offset - _appliedOffset;
        _appliedOffset = offset;
    }

    [HarmonyPatch(typeof(NTimelineScreen), "ResetScreen")]
    [HarmonyPostfix]
    private static void ResetEpochSlotContainerPosition(NTimelineScreen __instance)
    {
        if (_appliedOffset == Vector2.Zero) return;
        EpochSlotContainer(__instance).Position -= _appliedOffset;
        _appliedOffset = Vector2.Zero;
    }

    [HarmonyPatch(typeof(NTimelineScreen), nameof(NTimelineScreen.SetScreenDraggability))]
    [HarmonyPostfix]
    private static void AllowVerticalDragForTallColumns(NTimelineScreen __instance)
    {
        _allowVerticalDrag = TallestColumn(EpochSlotContainer(__instance)) > VisibleRows;
        if (_allowVerticalDrag) SlotsContainer(__instance).MouseFilter = Control.MouseFilterEnum.Stop;
    }

    [HarmonyPatch(typeof(NSlotsContainer), "ProcessPanEvent")]
    [HarmonyPrefix]
    private static void RememberDragStart(NSlotsContainer __instance, out Vector2 __state)
    {
        __state = DragStartPosition(__instance);
    }

    /// <summary>
    /// The game only applies the horizontal part of the drag.
    /// </summary>
    [HarmonyPatch(typeof(NSlotsContainer), "ProcessPanEvent")]
    [HarmonyPostfix]
    private static void AddVerticalDrag(NSlotsContainer __instance, InputEvent inputEvent, Vector2 __state)
    {
        if (!_allowVerticalDrag || inputEvent is not InputEventMouseMotion motion || !IsDragging(__instance)) return;
        TargetPosition(__instance) += new Vector2(0, (motion.Position - __state).Y);
    }

    [HarmonyPatch(typeof(NSlotsContainer), nameof(NSlotsContainer._Process))]
    [HarmonyPostfix]
    private static void LerpToMinMaxHeights(NSlotsContainer __instance, double delta)
    {
        if (!_allowVerticalDrag || !IsDragging(__instance)) return;

        ref var targetPosition = ref TargetPosition(__instance);
        var y = targetPosition.Y;
        var upperBound = EpochSlots(__instance).Size.Y - 700;
        const float lowerBound = -550;
        if (y > upperBound)
            y = Mathf.Lerp(y, upperBound, (float)delta * 36f);
        else if (y < lowerBound)
            y = Mathf.Lerp(y, lowerBound, (float)delta * 36f);
        targetPosition = new Vector2(targetPosition.X, y);
    }

    [HarmonyPatch(typeof(NSlotsContainer), nameof(NSlotsContainer.Reset))]
    [HarmonyPostfix]
    private static void ResetWhatsMovedYPosition(NSlotsContainer __instance)
    {
        if (!_allowVerticalDrag) return;
        var whatsMoved = WhatsMoved(__instance);
        whatsMoved.Position = new Vector2(whatsMoved.Position.X, -540f);
    }
}
