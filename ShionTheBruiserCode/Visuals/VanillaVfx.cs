using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Base-game hit effects that take more than a <c>WithHitFx</c> path, reproduced the way the vanilla cards play them.
/// Visual only, never touches the game state.
/// </summary>
public static class VanillaVfx
{
    /// <summary>Tint the base game uses for its magic missiles (Comet, Meteor Strike, Bombardment).</summary>
    public static readonly Color MissileTint = new("50b598");

    /// <summary>
    /// Explosive Ampoule: a fire and smoke puff on each target, then a short wait so the damage lands with the blast.
    /// Meant for <c>AttackCommand.BeforeDamage</c>.
    /// </summary>
    public static async Task FireSmokePuffs(IEnumerable<Creature> targets)
    {
        var container = NCombatRoom.Instance?.CombatVfxContainer;
        if (container == null) return;
        foreach (var target in targets.Where(t => t.IsAlive))
            container.AddChildSafely(NFireSmokePuffVfx.Create(target));
        await Cmd.CustomScaledWait(0.2f, 0.3f);
    }

    /// <summary>Comet / Guiding Star: a small magic missile falls on the target, awaited until it lands.</summary>
    public static async Task SmallMagicMissile(Creature target)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null) return;
        var missile = NSmallMagicMissileVfx.Create(node.GetBottomOfHitbox(), MissileTint);
        if (missile == null) return;
        NCombatRoom.Instance!.CombatVfxContainer.AddChildSafely(missile);
        await Cmd.Wait(missile.WaitTime);
    }

    /// <summary>
    /// Per-hit small magic missile, for <c>WithHitVfxNode</c> on multi-hit or random-target attacks. Pair it with
    /// <c>BeforeDamage(VanillaVfx.WaitForSmallMagicMissile)</c> so each hit lands with its missile.
    /// </summary>
    public static Node2D? SmallMagicMissileNode(Creature target)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(target);
        return node == null ? null : NSmallMagicMissileVfx.Create(node.GetBottomOfHitbox(), MissileTint);
    }

    /// <summary>Flight time of the small magic missile (<see cref="NSmallMagicMissileVfx.WaitTime"/>).</summary>
    public static Task WaitForSmallMagicMissile() => Cmd.Wait(0.2f);

    /// <summary>Meteor Strike / Bombardment: a large magic missile crashes on the target, awaited until it lands.</summary>
    public static async Task LargeMagicMissile(Creature target)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null) return;
        var missile = NLargeMagicMissileVfx.Create(node.GetBottomOfHitbox(), MissileTint);
        if (missile == null) return;
        NCombatRoom.Instance!.CombatVfxContainer.AddChildSafely(missile);
        await Cmd.Wait(missile.WaitTime);
    }
}
