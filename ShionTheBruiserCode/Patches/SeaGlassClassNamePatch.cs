using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

/// <summary>
/// Sea Glass (Orobas) names the character through its Title ("Shion, the Bruiser").
/// Our characters use their class name there instead, read from "SEA_GLASS.(Character ID).character" in relics.json ("The Bruiser").
/// Display only: the relic's saved CharacterId is untouched.
/// </summary>
[HarmonyPatch(typeof(SeaGlass), nameof(SeaGlass.CharacterId), MethodType.Setter)]
public static class SeaGlassClassNamePatch
{
    [HarmonyPostfix]
    public static void Postfix(SeaGlass __instance)
    {
        if (__instance.CharacterId == null) return;

        var key = $"{__instance.Id.Entry}.{__instance.CharacterId.Entry}.character";
        if (!LocString.Exists("relics", key)) return;

        ((StringVar)__instance.DynamicVars["Character"]).StringValue = new LocString("relics", key).GetFormattedText();
    }
}
