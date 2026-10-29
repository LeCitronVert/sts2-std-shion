using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// The Partner's copy of a Team-Up uses its ".partnerDescription" text, written from the Partner's side.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.Description), MethodType.Getter)]
public static class TeamUpPartnerDescriptionPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref LocString __result)
    {
        if (__instance is ShionTeamUpCard { IsPartnerCopy: true })
        {
            __result = new LocString("cards", __instance.Id.Entry + ".partnerDescription");
        }
    }
}

/// <summary>
/// Adds the "Team-Up: (Class)" line at the very top of Team-Up cards, above the other keywords.
/// It names the Partner's class, or the Initiator's class on the Partner's copy.
/// </summary>
[HarmonyPatch]
public static class TeamUpDescriptionPatch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.GetDeclaredMethods(typeof(CardModel))
            .Single(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);
    }

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref string __result)
    {
        if (__instance is not ShionTeamUpCard card) return;

        var line = new LocString("card_keywords", "SHIONTHEBRUISER-TEAMUP.cardText");
        line.Add("Class", TeamUpHelper.GetClassName(card.IsPartnerCopy ? card.InitiatorCharacter : card.PartnerCharacter));
        __result = string.IsNullOrEmpty(__result) ? line.GetFormattedText() : line.GetFormattedText() + "\n" + __result;
    }
}

/// <summary>
/// Every loot (rewards, shops, transforms...) filters its cards here.
/// Team-Ups are only offered when the run has both their Initiator's and their Partner's class.
/// </summary>
[HarmonyPatch(typeof(CardFactory), "FilterForPlayerCount")]
public static class TeamUpLootPatch
{
    [HarmonyPostfix]
    public static void Postfix(IRunState runState, ref IEnumerable<CardModel> __result)
    {
        __result = __result.Where(c => c is not ShionTeamUpCard teamUp || TeamUpHelper.IsAvailableInRun(teamUp, runState));
    }
}

/// <summary>
/// Neow sometimes offers Friendship Bracelet in place of her first blessing, when the player has Team-Ups available with the current team.
/// Odds match a regular blessing: Neow shows 2 out of ~16 positive options.
/// </summary>
[HarmonyPatch(typeof(Neow), "GenerateInitialOptions")]
public static class FriendshipBraceletNeowPatch
{
    private static readonly MethodInfo RelicOption = AccessTools.Method(typeof(AncientEventModel), "RelicOption",
        [typeof(RelicModel), typeof(string), typeof(string)]);

    private const float OfferChance = 0.125f;

    [HarmonyPostfix]
    public static void Postfix(Neow __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (__instance.Owner is not { } player || __result.Count == 0) return;
        if (player.RunState.Modifiers.Count > 0) return;
        if (!ModelDb.Relic<FriendshipBracelet>().IsAllowedAtNeow(player)) return;
        if (__instance.Rng.NextFloat() >= OfferChance) return;

        var options = __result.ToList();
        options[0] = (EventOption)RelicOption.Invoke(__instance,
            [ModelDb.Relic<FriendshipBracelet>().ToMutable(), "INITIAL", "NEOW.pages.DONE.POSITIVE.description"])!;
        __result = options;
    }
}
