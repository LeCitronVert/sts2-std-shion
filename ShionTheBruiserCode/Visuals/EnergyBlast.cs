using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
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
    /// <param name="onLand">Optional visual-only callback for each target when the blast lands, just before the damage.</param>
    /// <param name="speed">Speeds up the attack: travel time and attack animation delay are divided by it (Energy Volley).</param>
    public static AttackCommand WithEnergyBlast(this AttackCommand command, CardModel card, string? hitVfx = null,
        Action<Creature>? onLand = null, float speed = 1f)
    {
        var attacker = card.Owner.Creature;
        List<Creature> pendingTargets = [];
        speed = Math.Max(speed, 0.1f);

        return command
            .WithAttackerAnim(
                Character.ShionTheBruiser.GetEnergyAnimIfApplicable(card.Owner.Character),
                Character.ShionTheBruiser.GetEnergyDelayIfApplicable(card.Owner.Character) / speed)
            .WithHitVfxNode(target =>
            {
                pendingTargets.Add(target);
                return Create(attacker, target, speed);
            })
            .BeforeDamage(async () =>
            {
                if (pendingTargets.Count == 0) return;
                await Cmd.CustomScaledWait(FastTravelTime / speed, StandardTravelTime / speed);
                foreach (var target in pendingTargets.Where(t => t.IsAlive))
                {
                    if (hitVfx != null) VfxCmd.PlayOnCreatureCenter(target, hitVfx);
                    onLand?.Invoke(target);
                }
                pendingTargets.Clear();
            });
    }

    /// <summary>Duration of a <c>Cmd.CustomScaledWait(fast, standard)</c> with the current Fast Mode (0 when Instant).</summary>
    public static float ScaledTime(float fast, float standard) => SaveManager.Instance.PrefsSave.FastMode switch
    {
        FastModeType.Fast => fast,
        FastModeType.Normal => standard,
        _ => 0f,
    };

    private static float TravelTime => ScaledTime(FastTravelTime, StandardTravelTime);

    /// <summary>
    /// Throws a blast from the attacker to the target outside of an attack command (e.g. the Architect's victory
    /// scene). Visual only. Returns its travel time (0 when there's nothing to show, like in Instant mode).
    /// </summary>
    public static float Launch(Creature attacker, Creature target, float speed = 1f)
    {
        speed = Math.Max(speed, 0.1f);
        var container = NCombatRoom.Instance?.CombatVfxContainer;
        if (container == null || Create(attacker, target, speed) is not { } blast) return 0f;
        container.AddChild(blast);
        return TravelTime / speed;
    }

    private static Node2D? Create(Creature attacker, Creature target, float speed)
    {
        var travelTime = TravelTime / speed;
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
