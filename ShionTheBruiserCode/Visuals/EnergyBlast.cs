using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Energy Attacks throw an energy blast at each target, and deal their damage when it lands.
/// </summary>
public static class EnergyBlast
{
    private static readonly string ScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "energy_blast.tscn");

    // Travel time of a blast, matching Cmd.CustomScaledWait's fast/standard modes
    private const float FastTravelTime = 0.15f;
    private const float StandardTravelTime = 0.25f;

    // The blast leaves from slightly in front of the attacker
    private static readonly Vector2 LaunchOffset = new(90f, -20f);

    /// <summary>
    /// Replaces <c>WithAttackerAnim</c> for Energy Attacks: plays the Energy animation, throws a blast
    /// at every target of each hit, and waits for it to land before the damage is dealt.
    /// </summary>
    /// <param name="hitVfx">Optional VFX played on each target when the blast lands.</param>
    public static AttackCommand WithEnergyBlast(this AttackCommand command, CardModel card, string? hitVfx = null)
    {
        var attacker = card.Owner.Creature;
        List<Creature> pendingTargets = [];

        return command
            .WithAttackerAnim(
                Character.ShionTheBruiser.GetEnergyAnimIfApplicable(card.Owner.Character),
                Character.ShionTheBruiser.GetEnergyDelayIfApplicable(card.Owner.Character))
            .WithHitVfxNode(target =>
            {
                pendingTargets.Add(target);
                return Create(attacker, target);
            })
            .BeforeDamage(async () =>
            {
                if (pendingTargets.Count == 0) return;
                await Cmd.CustomScaledWait(FastTravelTime, StandardTravelTime);
                if (hitVfx != null)
                {
                    foreach (var target in pendingTargets.Where(t => t.IsAlive))
                    {
                        VfxCmd.PlayOnCreatureCenter(target, hitVfx);
                    }
                }
                pendingTargets.Clear();
            });
    }

    private static float TravelTime => SaveManager.Instance.PrefsSave.FastMode switch
    {
        FastModeType.Fast => FastTravelTime,
        FastModeType.Normal => StandardTravelTime,
        _ => 0f,
    };

    private static Node2D? Create(Creature attacker, Creature target)
    {
        var travelTime = TravelTime;
        if (travelTime <= 0f) return null;

        var attackerNode = attacker.GetCreatureNode();
        var targetNode = target.GetCreatureNode();
        if (attackerNode == null || targetNode == null) return null;

        var from = attackerNode.Visuals.VfxSpawnPosition.GlobalPosition + LaunchOffset;
        var to = targetNode.Visuals.VfxSpawnPosition.GlobalPosition;
        var spiritBurst = attacker.HasPower<SpiritBurst>();

        var blast = GD.Load<PackedScene>(ScenePath).Instantiate<Node2D>();
        // launch() needs the node to be in the tree to place it in global coordinates
        blast.Ready += () => blast.Call("launch", from, to, spiritBurst, travelTime);
        return blast;
    }
}
