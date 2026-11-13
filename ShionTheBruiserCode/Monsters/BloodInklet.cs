using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Monsters;

/// <summary>
/// Royal Mage's pet: the Inklet enemy's model with a red filter, facing the enemies.
/// Takes attack damage for its owner before Osty does (see <see cref="Powers.BloodInkletPower"/>).
/// </summary>
public class BloodInklet : CustomMonsterModel
{
    private static readonly string InkletVisualsPath = SceneHelper.GetScenePath("creature_visuals/inklet");

    private static readonly Color BloodTint = new(1f, 0.35f, 0.35f);

    public override int MinInitialHp => 1;

    public override int MaxInitialHp => 1;

#if STS2_BETA
    public override bool IsInteractable => Creature.IsAlive;
#else
    public override bool IsHealthBarVisible => Creature.IsAlive;
#endif

    public override string? CustomVisualPath => InkletVisualsPath;

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

    public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/inklet/inklet_hurt";

    /// <summary>
    /// Summons a Blood Inklet for the player with this much Max HP (at least 1) and Slippery.
    /// If they already have a living one, it gains that much Max HP and Slippery instead.
    /// </summary>
    public static async Task Summon(PlayerChoiceContext choiceContext, Player player, decimal maxHp, decimal slippery, CardModel? source)
    {
        if (player.PlayerCombatState is not { } combatState || player.Creature.IsDead) return;
        maxHp = Math.Max(1m, maxHp);

        var inklet = combatState.Pets.FirstOrDefault(p => p.Monster is BloodInklet && p.IsAlive);
        if (inklet != null)
        {
            await CreatureCmd.GainMaxHp(inklet, maxHp);
        }
        else
        {
            inklet = await PlayerCmd.AddPet<BloodInklet>(player);
            // The combat room makes every pet but Osty non-interactable (no hover, no health bar), like Byrdpip
#if STS2_BETA
            NCombatRoom.Instance?.GetCreatureNode(inklet)?.SetIsInteractable(true, true);
#else
            NCombatRoom.Instance?.SetCreatureIsInteractable(inklet, true);
#endif
            await PowerCmd.Apply<BloodInkletPower>(choiceContext, inklet, 1, player.Creature, source);
            await CreatureCmd.SetMaxHp(inklet, maxHp);
            await CreatureCmd.Heal(inklet, maxHp, false);
        }

        await PowerCmd.Apply<SlipperyPower>(choiceContext, inklet, slippery, player.Creature, source);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var initialState = new MoveState("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);
        initialState.FollowUpState = initialState;
        return new MonsterMoveStateMachine([initialState], initialState);
    }

    public override NCreatureVisuals CreateCustomVisuals()
    {
        // Loaded directly: it can be summoned mid-combat, so it may not be in the preload cache
        var visuals = ResourceLoader.Load<PackedScene>(InkletVisualsPath, null, ResourceLoader.CacheMode.Reuse)
            .Instantiate<NCreatureVisuals>(PackedScene.GenEditState.Disabled);
        visuals.Modulate = BloodTint;

        // Inklets face the players: flip the body so it faces the enemies
        if (visuals.GetNodeOrNull<Node2D>("%Visuals") is { } body)
        {
            body.Scale = new Vector2(-body.Scale.X, body.Scale.Y);
        }

        return visuals;
    }
}
