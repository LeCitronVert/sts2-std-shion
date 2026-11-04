using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Writes "Maim X." at the top of the text of any card with Maim, right below its "before" keywords (Wounding, Retain...).
/// Covers both cards with Maim printed on them and cards Maim was added to (Go Crazy, Steal Blood).
/// </summary>
[HarmonyPatch]
public static class MaimDescriptionPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.GetDeclaredMethods(typeof(CardModel))
            .First(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref string __result)
    {
        var maim = MaimHelper.GetMaim(__instance);
        if (maim <= 0) return;

        var line = new LocString("card_keywords", "SHIONTHEBRUISER-MAIM.cardText");
        line.Add("Amount", FormatAmount(__instance, maim));
        var text = line.GetFormattedText();

        var lines = __result.Split('\n').ToList();
        var keywordLines = __instance.Keywords
            .Select(k => "[gold]" + new LocString("card_keywords", StringHelper.Slugify(k.ToString()) + ".title").GetFormattedText() + "[/gold]")
            .ToList();
        var index = 0;
        while (index < lines.Count && keywordLines.Any(k => lines[index].StartsWith(k) && lines[index].Length <= k.Length + 1))
        {
            index++;
        }
        lines.Insert(index, text);
        __result = string.Join('\n', lines);
    }

    /// <summary>Same colours as the {Var:diff()} formatter: green when the amount went up (upgrade or added Maim).</summary>
    private static string FormatAmount(CardModel card, int total)
    {
        if (MaimHelper.GetAddedMaim(card) > 0) return $"[green]{total}[/green]";
        return card.DynamicVars.TryGetValue(MaimHelper.VarName, out var native)
            ? native.ToHighlightedString(false)
            : $"[blue]{total}[/blue]";
    }
}
