using ShionTheBruiser.ShionTheBruiserCode.Strife;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Base class of the Chaos Emerald cards (all Powers). The Emerald Power keyword: gain 10 Rings; playing an Emerald Power
/// first removes the active Emerald Power effect (so only one is active, and Water can't copy them); always Retained.
/// They cost energy, except when picked from the Super Form Emerald wheel (<see cref="Strife.EmeraldWheel"/>), which makes
/// that copy free. Each Chaos Emerald adds its card to the Draw Pile at the start of combat; with Super Form, the card is
/// picked from the wheel instead.
/// </summary>
public abstract class EmeraldPowerCard(int cost, TargetType target) :
    ShionTheBruiserCard(cost, CardType.Power, CardRarity.Token, target)
{
    public const int RingsGained = 10;

    public abstract ChaosEmeraldColor Color { get; }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.EmeraldPower];
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    /// <summary>Picked from the Emerald wheel: free to play. Set on every client by the wheel's action.</summary>
    public bool FromWheel { get; set; }

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || !FromWheel) return false;
        modifiedCost = 0m;
        return true;
    }

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await EmeraldPowerEffect.RemoveAll(creature);
        await Rings.Gain(choiceContext, creature, RingsGained);
        await OnEmeraldPlay(choiceContext, play);
    }

    protected abstract Task OnEmeraldPlay(PlayerChoiceContext choiceContext, CardPlay play);
}

/// <summary>Emerald Powers are always Retained (see <see cref="EmeraldPowerCard"/>).</summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.ShouldRetainThisTurn), MethodType.Getter)]
public static class EmeraldPowerRetainPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__instance is EmeraldPowerCard) __result = true;
    }
}
