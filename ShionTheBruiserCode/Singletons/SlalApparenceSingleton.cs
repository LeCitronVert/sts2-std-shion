using BaseLib.Abstracts;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Singletons;

/// <summary>
/// Slal shrinks as he loses HP. Combined with Shrink (Shrinking Beetle...), which halves that size instead of
/// overwriting it (see <see cref="SlalShrinkScalePatch"/>).
/// </summary>
public class SlalApparenceSingleton(): CustomSingletonModel(HookType.Combat)
{
    private const float MinScale = 0.4f;
    private const float MaxScale = 1f;
    /// <summary>The base game's Shrink size.</summary>
    private const float ShrinkScale = 0.5f;
    
    public override async Task AfterCurrentHpChanged(Creature creature, Decimal delta)
    {
        await base.AfterCurrentHpChanged(creature, delta);
        if (creature.Player?.Character is not SlalTheEndless) return;
        UpdateScale(creature);
    }

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();
        // DebugOnlyGetState just returns the live combat state; BeforeCombatStart has no other way to reach it.
        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null) return;
        foreach (var creature in combatState.Allies)
        {
            if (creature.Player?.Character is SlalTheEndless) UpdateScale(creature);
        }
    }

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        await base.AfterCreatureAddedToCombat(creature);
        if (creature.Player?.Character is SlalTheEndless) UpdateScale(creature);
    }

    /// <summary>Size from Slal's HP, halved while he has Shrink (Shrink's own size).</summary>
    public static void UpdateScale(Creature creature)
    {
        float ratio = creature.MaxHp > 0
            ? Mathf.Clamp((float)creature.CurrentHp / creature.MaxHp, 0f, 1f)
            : 0f;
        float scale = Mathf.Lerp(MinScale, MaxScale, ratio);
        if (creature.HasPower<ShrinkPower>()) scale *= ShrinkScale;
        NCombatRoom.Instance?.GetCreatureNode(creature)?.ScaleTo(scale, 0.4);
    }
}
/// <summary>
/// Shrink sets its target's size to 0.5 (and back to 1 when removed), which overwrote Slal's HP-based size.
/// After it does, Slal's combined size is set again. Visual only.
/// </summary>
[HarmonyPatch(typeof(ShrinkPower))]
public static class SlalShrinkScalePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShrinkPower.AfterApplied))]
    public static void AfterApplied(ShrinkPower __instance) => Refresh(__instance.Owner);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShrinkPower.AfterRemoved))]
    public static void AfterRemoved(Creature oldOwner) => Refresh(oldOwner);

    private static void Refresh(Creature? creature)
    {
        if (creature?.Player?.Character is SlalTheEndless) SlalApparenceSingleton.UpdateScale(creature);
    }
}
