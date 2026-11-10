using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Act 4: Strife. Never rolled at run start (act number -1): <see cref="StrifeTransition"/> appends it to the run after
/// the Act 3 Architect, when every player holds a combo "Chaos Emeralds" relic.
/// A single thread: shadowed Chaos Emeralds (full heal + Gold) → Merchant → Rest Site → the Architect.
/// Reuses Glory's map and rest site art. Its music is handled by <see cref="StrifeMusic"/>.
/// </summary>
public sealed class StrifeAct() : CustomActModel(-1)
{
    private const string MapArtAct = "glory";

    public override IEnumerable<EncounterModel> GenerateAllEncounters() => [ModelDb.Encounter<StrifeArchitectEncounter>()];

    public override IEnumerable<EventModel> AllEvents => [];

    public override IEnumerable<AncientEventModel> AllAncients => [ModelDb.AncientEvent<StrifeShrine>()];

    public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState state) => AllAncients;

    public override Color MapTraveledColor => new("141018");
    public override Color MapUntraveledColor => new("4c4a5c");
    public override Color MapBgColor => new("6d6a7e");

    protected override string CustomMapTopBgPath => ImageHelper.GetImagePath($"packed/map/map_bgs/{MapArtAct}/map_top_{MapArtAct}.png");
    protected override string CustomMapMidBgPath => ImageHelper.GetImagePath($"packed/map/map_bgs/{MapArtAct}/map_middle_{MapArtAct}.png");
    protected override string CustomMapBotBgPath => ImageHelper.GetImagePath($"packed/map/map_bgs/{MapArtAct}/map_bottom_{MapArtAct}.png");
    protected override string CustomRestSiteBackgroundPath => SceneHelper.GetScenePath($"rest_site/{MapArtAct}_rest_site");

    protected override BackgroundAssets CustomGenerateBackgroundAssets(Rng rng) => new(MapArtAct, rng);

    protected override ActMap? CustomCreateMap(RunState runState, bool replaceTreasureWithElites) => new StrifeActMap();

    public static bool IsStrife(ActModel? act) => act?.Id == ModelDb.Act<StrifeAct>().Id;

    public static bool IsStrife(IRunState? runState) => runState != null && runState.Acts.Count > 0 && IsStrife(runState.Act);
}

/// <summary>Shadowed Chaos Emeralds (start) → Merchant → Rest Site → Boss, in a single column.</summary>
public sealed class StrifeActMap : ActMap
{
    private const int Width = 7;
    private const int Middle = 3;

    private static readonly MapPointType[] Path = [MapPointType.Shop, MapPointType.RestSite];

    public override MapPoint BossMapPoint { get; }

    public override MapPoint StartingMapPoint { get; }

    protected override MapPoint?[,] Grid { get; }

    public StrifeActMap()
    {
        Grid = new MapPoint[Width, Path.Length + 1];
        BossMapPoint = new MapPoint(Middle, GetRowCount()) { PointType = MapPointType.Boss, CanBeModified = false };
        StartingMapPoint = new MapPoint(Middle, 0) { PointType = MapPointType.Ancient, CanBeModified = false };

        for (var i = 0; i < Path.Length; i++)
        {
            var point = new MapPoint(Middle, i + 1) { PointType = Path[i], CanBeModified = false };
            Grid[Middle, i + 1] = point;
            if (i > 0) Grid[Middle, i]!.AddChildPoint(point);
        }

        startMapPoints.Add(Grid[Middle, 1]!);
        StartingMapPoint.AddChildPoint(Grid[Middle, 1]!);
        Grid[Middle, GetRowCount() - 1]!.AddChildPoint(BossMapPoint);
    }
}
