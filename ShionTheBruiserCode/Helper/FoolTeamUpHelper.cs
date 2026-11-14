#if STS2_BETA
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Shared code for the Team-Ups that use the Fool's Arrows (beta only: the Fool doesn't exist on the main branch).
/// </summary>
public static class FoolTeamUpHelper
{
    /// <summary>
    /// A card's base damage ("its base damage" in card text): its Damage var, or its calculated damage.
    /// Null when the card doesn't deal damage.
    /// </summary>
    public static decimal? GetBaseDamage(CardModel? card, Creature? target = null)
    {
        if (card == null) return null;
        if (card.DynamicVars.TryGetValue("Damage", out var damage)) return damage.BaseValue;
        if (card.DynamicVars.TryGetValue("CalculatedDamage", out var calculated) && calculated is CalculatedVar calc) return calc.Calculate(target);
        return null;
    }

    /// <summary>A card's base Block: its Block var, or its calculated Block. Null when the card doesn't give Block.</summary>
    public static decimal? GetBaseBlock(CardModel? card)
    {
        if (card == null) return null;
        if (card.DynamicVars.TryGetValue("Block", out var block)) return block.BaseValue;
        if (card.DynamicVars.TryGetValue("CalculatedBlock", out var calculated) && calculated is CalculatedVar calc) return calc.Calculate(null);
        return null;
    }

    /// <summary>
    /// Gives the player an Arrow slot if they have none, like the game does for a Shoot card entering a non-Fool's piles.
    /// </summary>
    public static void EnsureArrowSlot(Player player)
    {
        if (player.PlayerCombatState is { Arrows.Count: 0 })
        {
            ArrowCmd.GainArrowSlot(player);
        }
    }

    /// <summary>
    /// The player Shoots an Arrow at the target, dealing the damage as an attack from the card (Aim applies).
    /// Does nothing when they have no Arrow available. Returns whether an Arrow was shot.
    /// </summary>
    public static async Task<bool> ShootArrow(PlayerChoiceContext choiceContext, Player shooter, CardModel card, CardPlay cardPlay,
        Creature target, decimal damage)
    {
        if (target.IsDead || !ArrowCmd.PlayerHasAnyArrows(shooter)) return false;

        var attack = DamageCmd.Attack(damage).FromArrow().FromCard(card, cardPlay)
            .WithAttackerAnim(Fool.GetShootBowAnimIfApplicable(shooter.Character), Fool.GetShootBowDelayIfApplicable(shooter.Character))
            .WithAttackerFx(null, Fool.arrowShootSfx)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash");
        return await ArrowCmd.ShootArrowAtTarget(target, shooter, attack, choiceContext);
    }

    /// <summary>The given enemy if it can still be hit, otherwise a random hittable enemy (null if there are none).</summary>
    public static Creature? TargetOrRandom(Player player, Creature? target)
    {
        if (target is { IsAlive: true, IsHittable: true }) return target;
        var enemies = player.Creature.CombatState?.HittableEnemies.ToList() ?? [];
        return enemies.Count == 0 ? null : player.RunState.Rng.CombatTargets.NextItem(enemies);
    }
}
#endif
