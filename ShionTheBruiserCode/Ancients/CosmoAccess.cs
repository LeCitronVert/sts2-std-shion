using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Ancients;

/// <summary>Private game members used by Cosmo and her relics.</summary>
public static class CosmoAccess
{
    public static readonly AccessTools.FieldRef<ActModel, RoomSet> Rooms =
        AccessTools.FieldRefAccess<ActModel, RoomSet>("_rooms");

    public static readonly AccessTools.FieldRef<NAncientEventLayout, AncientEventModel> AncientEvent =
        AccessTools.FieldRefAccess<NAncientEventLayout, AncientEventModel>("_ancientEvent");

    public static readonly AccessTools.FieldRef<CardReward, bool> HasBeenRerolled =
        AccessTools.FieldRefAccess<CardReward, bool>("_hasBeenRerolled");

    public static readonly AccessTools.FieldRef<CardReward, List<CardCreationResult>> RewardCards =
        AccessTools.FieldRefAccess<CardReward, List<CardCreationResult>>("_cards");

    public static readonly AccessTools.FieldRef<SmithRestSiteOption, IEnumerable<CardModel>?> SmithSelection =
        AccessTools.FieldRefAccess<SmithRestSiteOption, IEnumerable<CardModel>?>("_selection");

    private static readonly Func<RestSiteOption, Player> RestSiteOwnerGetter =
        AccessTools.MethodDelegate<Func<RestSiteOption, Player>>(AccessTools.PropertyGetter(typeof(RestSiteOption), "Owner"));

    private static readonly Func<RunManager, RunState?> RunStateGetter =
        AccessTools.MethodDelegate<Func<RunManager, RunState?>>(AccessTools.PropertyGetter(typeof(RunManager), "State"));

    public static Player OwnerOf(RestSiteOption option) => RestSiteOwnerGetter(option);

    public static RunState? StateOf(RunManager manager) => RunStateGetter(manager);
}
