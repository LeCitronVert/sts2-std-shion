using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (cursed). When your HP would reach 0, heal to 50% of your Max HP instead (once, like Lizard Tail).
/// Your cards have no description (only on your own copies of the cards: the card text is blanked).
/// </summary>
public class InvisibleNotebook : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    public override bool IsCursedOption => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(50m)];

    private bool _wasUsed;

    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
        }
    }

    public override bool ShouldDieLate(Creature creature) => creature != Owner.Creature || WasUsed;

    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();
        WasUsed = true;
        await CreatureCmd.Heal(creature, Math.Max(1m, creature.MaxHp * (DynamicVars.Heal.BaseValue / 100m)));
    }

    public static bool HidesTextOf(CardModel card) =>
        card.IsMutable && card.Owner?.Relics.Any(r => r is InvisibleNotebook) == true;
}

/// <summary>Every card description (hand, deck, rewards...) goes through this.</summary>
[HarmonyPatch]
public static class InvisibleNotebookDescriptionPatch
{
    // The private overload (PileType, DescriptionPreviewType, Creature) that the public ones call
    public static System.Reflection.MethodBase TargetMethod() =>
        AccessTools.GetDeclaredMethods(typeof(CardModel)).First(m => m.Name == "GetDescriptionForPile" && m.GetParameters().Length == 3);

    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref string __result)
    {
        if (InvisibleNotebook.HidesTextOf(__instance)) __result = "";
    }
}
