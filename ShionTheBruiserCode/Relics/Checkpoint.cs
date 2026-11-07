using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (solo). Once: when choosing where to travel next, you may also go back to the last Rest Site you visited
/// this act. Used up when you actually go back (seen like Winged Boots: the room you entered isn't a child of the last one).
/// </summary>
public class Checkpoint : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.SoloOnly;

    private bool _wasUsed;

    public override bool IsUsedUp => _wasUsed;

    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (_wasUsed) Status = RelicStatus.Disabled;
        }
    }

    /// <summary>The last Rest Site visited this act before <paramref name="current"/>.</summary>
    public static MapPoint? LastRestSite(RunState runState, MapPoint current)
    {
        var visited = runState.VisitedMapCoords;
        for (var i = visited.Count - 1; i >= 0; i--)
        {
            if (visited[i] == current.coord) continue;
            if (runState.Map.GetPoint(visited[i]) is { PointType: MapPointType.RestSite } point) return point;
        }
        return null;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (WasUsed || Owner.RunState is not RunState runState || runState.VisitedMapCoords.Count <= 1) return Task.CompletedTask;
        if (Owner.RunState.CurrentRoomCount > 1) return Task.CompletedTask;

        var visited = runState.VisitedMapCoords;
        var current = visited[^1];
        // Going back = a lower row than the room before (Secret Key moves sideways, on the same row)
        if (current.row >= visited[^2].row) return Task.CompletedTask;

        Flash();
        WasUsed = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Extra map destinations for the solo travel relics (Checkpoint, Secret Key).
/// The map screen marks whatever this returns as travelable.
/// </summary>
[HarmonyPatch(typeof(MapTravel), nameof(MapTravel.GetTravelablePointsFrom))]
public static class CosmoTravelPatch
{
    [HarmonyPostfix]
    public static void Postfix(IRunState runState, MapPoint currentPoint, ref IEnumerable<MapPoint> __result)
    {
        if (runState is not RunState state || state.Players.Count != 1) return;
        var player = state.Players[0];
        List<MapPoint> extra = [];

        if (player.Relics.OfType<Checkpoint>().Any(r => !r.WasUsed) && Checkpoint.LastRestSite(state, currentPoint) is { } restSite)
            extra.Add(restSite);

        if (player.Relics.OfType<SecretKey>().Any(r => !r.WasUsed) && currentPoint.PointType == MapPointType.Treasure)
            extra.AddRange(SecretKey.OtherChests(state, currentPoint));

        if (extra.Count == 0) return;
        __result = __result.Concat(extra).Distinct().ToList();
    }
}
