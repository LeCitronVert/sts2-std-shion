using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Eternal added to a Deck card for the rest of the run (Chaos Ring, Halloween Party).
/// Card keywords aren't saved with the run, so the card also gets a saved flag, and Eternal is added back when it's loaded.
/// </summary>
public static class PermanentEternal
{
    public static readonly SavedSpireField<CardModel, bool> Field = new(() => false, "DreamersPermanentEternal");

    public static void Add(CardModel card)
    {
        Field.Set(card, true);
        if (!card.Keywords.Contains(CardKeyword.Eternal)) card.AddKeyword(CardKeyword.Eternal);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable))]
public static class PermanentEternalLoadPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __result)
    {
        if (__result != null && PermanentEternal.Field.Get(__result) && !__result.Keywords.Contains(CardKeyword.Eternal))
        {
            __result.AddKeyword(CardKeyword.Eternal);
        }
    }
}
