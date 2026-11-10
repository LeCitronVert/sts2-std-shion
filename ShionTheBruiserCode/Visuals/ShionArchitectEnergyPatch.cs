using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// The base game's Architect (end of Act 3): when Shion attacks him, she throws energy blasts like her Energy cards
/// (Energy animation, one blast per hit, the damage number when it lands) instead of the default blunt hits.
/// Same number of hits and the same damage split as the base game. Visual only.
/// </summary>
[HarmonyPatch(typeof(TheArchitect), "AnimPlayerAttackIfNecessary")]
public static class ShionArchitectEnergyPatch
{
    /// <summary>Blasts fly faster than an Energy card's, so the 20 hits don't drag on.</summary>
    private const float BlastSpeed = 2f;

    private static readonly Func<TheArchitect, Creature?> ArchitectGetter =
        AccessTools.MethodDelegate<Func<TheArchitect, Creature?>>(AccessTools.PropertyGetter(typeof(TheArchitect), "ArchitectCreature"));
    private static readonly Func<TheArchitect, int> ScoreGetter =
        AccessTools.MethodDelegate<Func<TheArchitect, int>>(AccessTools.PropertyGetter(typeof(TheArchitect), "Score"));
    private static readonly Func<EventModel, Rng> RngGetter =
        AccessTools.MethodDelegate<Func<EventModel, Rng>>(AccessTools.PropertyGetter(typeof(EventModel), "Rng"));
    private static readonly Func<int, int, Rng, int[]> DivideWildly =
        AccessTools.MethodDelegate<Func<int, int, Rng, int[]>>(AccessTools.Method(typeof(TheArchitect), "DivideWildly"));

    [HarmonyPrefix]
    public static bool Prefix(TheArchitect __instance, ArchitectAttackers attackers, ref Task<bool> __result)
    {
        if (__instance.Owner?.Character is not Character.ShionTheBruiser) return true;
        if (attackers != ArchitectAttackers.Player && attackers != ArchitectAttackers.Both) return true;
        if (ArchitectGetter(__instance) is not { } architect) return true;

        __result = EnergyAttack(__instance, architect);
        return false;
    }

    private static async Task<bool> EnergyAttack(TheArchitect architectEvent, Creature architect)
    {
        var owner = architectEvent.Owner!;
        var shion = owner.Creature;
        var hits = owner.Character.GetArchitectAttackVfx().Count;
        // Same RNG use as the base game (shuffle, then the split), so the numbers match it
        var rng = RngGetter(architectEvent);
        var order = Enumerable.Range(0, hits).ToList();
        rng.Shuffle(order);
        var damage = DivideWildly(ScoreGetter(architectEvent), hits, rng);
        var vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        var anim = Character.ShionTheBruiser.GetEnergyAnimIfApplicable(owner.Character);
        var animDelay = Character.ShionTheBruiser.GetEnergyDelayIfApplicable(owner.Character) / BlastSpeed;

        for (var i = 0; i < hits; i++)
        {
            var isFinalHit = i == hits - 1;
            await CreatureCmd.TriggerAnim(shion, anim, animDelay);

            var travel = EnergyBlast.Launch(shion, architect, BlastSpeed);
            if (travel > 0f) await Cmd.Wait(travel);

            vfxContainer?.AddChildSafely(NDamageNumVfx.Create(architect, damage[i], requireInteractable: false));
            vfxContainer?.AddChildSafely(NHitSparkVfx.Create(architect, requireInteractable: false));
            await CreatureCmd.TriggerAnim(architect, "Hit", 0f);
            if (isFinalHit) NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
            else NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
        }

        await Cmd.Wait(2f);
        return true;
    }
}
