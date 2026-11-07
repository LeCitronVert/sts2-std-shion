using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (solo). Once: after opening a Treasure chest, you may travel to another Treasure chest of the same row
/// (the act's half-way chests) instead of moving on. The map side is in <see cref="CosmoTravelPatch"/>.
/// Incompatible with Festive Ribbons.
/// </summary>
public class SecretKey : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.SoloOnly;

    public override IEnumerable<Type> IncompatibleOptions => [typeof(FestiveRibbons)];

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

    /// <summary>Unvisited Treasure points on the same row as <paramref name="chest"/>.</summary>
    public static IEnumerable<MapPoint> OtherChests(RunState runState, MapPoint chest) =>
        runState.Map.GetPointsInRow(chest.coord.row)
            .Where(p => p != chest && p.PointType == MapPointType.Treasure && !runState.VisitedMapCoords.Contains(p.coord));

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (WasUsed || room is not TreasureRoom || Owner.RunState is not RunState runState || runState.VisitedMapCoords.Count <= 1)
            return Task.CompletedTask;

        var visited = runState.VisitedMapCoords;
        if (visited[^1].row != visited[^2].row) return Task.CompletedTask;
        if (runState.Map.GetPoint(visited[^2]) is not { PointType: MapPointType.Treasure }) return Task.CompletedTask;

        Flash();
        WasUsed = true;
        return Task.CompletedTask;
    }
}
