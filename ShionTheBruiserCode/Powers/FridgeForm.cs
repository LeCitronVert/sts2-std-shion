using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Whenever you play a Form Power, procure X random potions.
/// At the end of combat, lose X potion slots and ALL potions.
/// Amount is both the number of potions procured and the number of slots gained by Fridge Form cards.
/// </summary>
public class FridgeForm : SlalOnPowerPlayedPower
{
    protected override bool ShouldTrigger(CardModel card) => SlalMechanicsHelper.IsFormPower(card);

    public static async Task ProcureRandomPotion(Player player)
    {
        await PotionCmd.TryToProcure(
            PotionFactory.CreateRandomPotionInCombat(player, player.RunState.Rng.CombatPotionGeneration).ToMutable(),
            player
        );
    }

    /// <summary>
    /// Procures random potions until every potion slot is full.
    /// </summary>
    public static async Task FillPotionSlots(Player player)
    {
        // Bounded in case a potion can't be procured
        var openSlots = player.PotionSlots.Count(p => p == null);
        for (var i = 0; i < openSlots && player.HasOpenPotionSlots; i++)
        {
            await ProcureRandomPotion(player);
        }
    }

    protected override async Task OnPowerPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay, int amount)
    {
        if (Owner.Player is null) return;
        for (var i = 0; i < amount; i++)
        {
            await ProcureRandomPotion(Owner.Player);
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner.Player;
        if (player is null) return;

        foreach (var potion in player.Potions.ToList())
        {
            await PotionCmd.Discard(potion);
        }
        await PlayerCmd.LoseMaxPotionCount(Math.Min(Amount, player.MaxPotionCount), player);
    }
}
