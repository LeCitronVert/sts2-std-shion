using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Confusion executions: instead of its death animation, the enemy is launched to the right and slightly up,
/// spinning clockwise (almost a full turn) until it leaves the screen, after a puff of smoke.
/// Purely visual, so it doesn't affect multiplayer sync.
/// </summary>
public static class ConfusionSpinOff
{
    /// <summary>Time it takes to leave the screen.</summary>
    private const double Duration = 0.9;

    /// <summary>Turns done before leaving the screen.</summary>
    private const float Turns = 0.9f;

    /// <summary>How much it goes up for each pixel it goes right.</summary>
    private const float Rise = 0.2f;

    /// <summary>Extra distance past the screen's edge, so the whole body is gone.</summary>
    private const float OffscreenMargin = 200f;

    /// <summary>Creatures being executed by Confusion right now. Their death plays the spin-off.</summary>
    private static readonly HashSet<Creature> Pending = [];

    /// <summary>
    /// Kills the creature with the spin-off animation (if it dies and is removed from combat).
    /// </summary>
    public static async Task Kill(Creature creature)
    {
        Pending.Add(creature);
        try
        {
            await CreatureCmd.Kill(creature);
        }
        finally
        {
            Pending.Remove(creature);
        }
    }

    [HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
    public static class StartDeathAnimPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NCreature __instance, bool shouldRemove, ref float __result)
        {
            if (!shouldRemove || TestMode.IsOn || !Pending.Contains(__instance.Entity)) return true;
            if (__instance.DeathAnimationTask is { IsCompleted: false }) return true;

            if (__instance.Hitbox.HasFocus())
            {
                ActiveScreenContext.Instance.FocusOnDefaultControl();
            }
            __instance.Hitbox.FocusMode = Control.FocusModeEnum.None;

            var monster = __instance.Entity.Monster;
            if (monster is { HasDeathSfx: true })
            {
                SfxCmd.PlayDeath(monster);
            }

            __instance.DeathAnimationTask = SpinOff(__instance, __instance.DeathAnimCancelToken.Token);
            TaskHelper.RunSafely(__instance.DeathAnimationTask);
            __result = (float) Duration;
            return false;
        }
    }

    private static async Task SpinOff(NCreature node, CancellationToken cancelToken)
    {
        node.AnimDisableUi();
        node.AnimHideIntent();
        node.Hitbox.MouseFilter = Control.MouseFilterEnum.Ignore;

        // Like the gremlins running away
        var puff = NSmokePuffVfx.Create(node.Entity, NSmokePuffVfx.SmokePuffColor.Green);
        if (puff != null) NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(puff);

        var visuals = node.Visuals;

        // Everything is computed in the visuals' parent space, so it works for mirrored creatures too
        var parentTransform = ((CanvasItem) visuals.GetParent()).GetGlobalTransform();
        var startPosition = visuals.Position;
        var startRotation = visuals.Rotation;
        var center = parentTransform.AffineInverse() * node.VfxSpawnPosition;
        var centerOffset = center - startPosition;

        // Right (and slightly up) on screen, far enough to leave it
        var screenRight = node.GetCanvasTransform().AffineInverse() * new Vector2(node.GetViewportRect().End.X, 0f);
        var bodySize = visuals.Bounds.Size.Length() * Mathf.Abs(visuals.GetGlobalTransform().Scale.X);
        var distance = Mathf.Max(screenRight.X - node.VfxSpawnPosition.X, 0f) + bodySize / 2f + OffscreenMargin;
        var move = parentTransform.BasisXformInv(new Vector2(distance, -distance * Rise));
        // Clockwise on screen, even if the parent is mirrored
        var spin = Mathf.Tau * Turns * (parentTransform.Determinant() < 0 ? -1f : 1f);

        var tween = node.CreateTween();
        tween.TweenMethod(Callable.From<float>(t =>
        {
            var angle = spin * t;
            // Spins around the body's center while that center moves
            visuals.Rotation = startRotation + angle;
            visuals.Position = center + move * t - centerOffset.Rotated(angle);
        }), 0f, 1f, Duration);

        await node.ToSignal(tween, Tween.SignalName.Finished);
        if (cancelToken.IsCancellationRequested) return;

        node.QueueFreeSafely();
    }
}
