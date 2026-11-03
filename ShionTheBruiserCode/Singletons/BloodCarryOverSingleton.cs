using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Singletons;

/// <summary>
/// Unspent Blood carries over between combats.
/// When a combat is won, each player's Blood is saved with the run; it's given back at the start of their
/// first turn of the next combat. Losing a combat ends the run, so only victories matter.
/// The saved value is the same on every client (Blood is part of the simulated combat state).
/// </summary>
public class BloodCarryOverSingleton() : CustomSingletonModel(HookType.Combat)
{
    /// <summary>Blood left at the end of the last combat won, per player.</summary>
    public static readonly SavedSpireField<Player, int> SavedBlood = new(() => 0, "SeelsSavedBlood");

    /// <summary>True while the carried-over Blood is being given back, so it isn't counted as Blood gained (Red Fang).</summary>
    public static bool IsRestoring { get; private set; }

    // Must be AfterCombatEnd, not AfterCombatVictory(Early): CombatManager.EndCombatInternal calls
    // Player.AfterCombatEnd() (which removes all powers, Blood included) between the two hooks.
    // AfterCombatEnd only runs on victory (a loss goes through LoseCombat instead).
    public override Task AfterCombatEnd(CombatRoom room)
    {
        foreach (var player in room.CombatState.Players)
        {
            SavedBlood.Set(player, player.Creature.IsAlive ? SeelsMechanicsHelper.GetBlood(player.Creature) : 0);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        var blood = SavedBlood.Get(player);
        if (blood <= 0) return;

        SavedBlood.Set(player, 0);
        IsRestoring = true;
        try
        {
            await SeelsMechanicsHelper.GainBlood(choiceContext, player.Creature, blood);
        }
        finally
        {
            IsRestoring = false;
        }
    }
}
