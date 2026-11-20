using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using BaseLib.Utils;
using HarmonyLib;
using System.Reflection;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Our Mages Are Different (Team-Up), on Fry.
/// Whenever you lose HP, gain Amount Stars.
/// Whenever a Partner plays a card that Blood Bends X (X ≥ 1), it gets Cooldown X: it returns to their Hand with Retain
/// and is free for X turns; played during those turns, it's discarded with no effect (OurMagesCooldownPatch).
/// </summary>
public class OurMagesAreDifferent : TeamUpPower, ISeelsListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(FryCardKeywords.Cooldown)];

    private readonly Dictionary<CardModel, int> _pending = [];

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner || delta >= 0 || Owner.IsDead || Owner.Player is not { } player) return;

        Flash();
        await PlayerCmd.GainStars(Amount, player);
    }

    public Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source)
    {
        if (source is null || x <= 0 || !LivingPartners.Contains(player.Creature) || OurMagesCooldown.IsOnCooldown(source)) return Task.CompletedTask;
        _pending[source] = x;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!_pending.TryGetValue(card, out var x)) return;
        if (cardPlay.PlayIndex < cardPlay.PlayCount - 1) return;
        _pending.Remove(card);

        // Exhausted or Power cards don't come back.
        if (card.Pile?.Type is not PileType.Discard) return;

        Flash();
        OurMagesCooldown.Start(card, x);
        await CardPileCmd.Add(card, PileType.Hand);
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!OurMagesCooldown.IsOnCooldown(card) || !Partners.Contains(card.Owner.Creature)) return false;
        modifiedCost = 0;
        return true;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!Partners.Contains(player.Creature)) return Task.CompletedTask;

        foreach (var card in player.PlayerCombatState?.AllCards.Where(OurMagesCooldown.IsOnCooldown).ToList() ?? [])
        {
            OurMagesCooldown.Tick(card);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Cooldown given to Seels' Blood Bend cards by Our Mages Are Different.
/// </summary>
public static class OurMagesCooldown
{
    private static readonly SpireField<CardModel, int> TurnsLeft = new(() => 0);
    private static readonly SpireField<CardModel, bool> AddedRetain = new(() => false);

    public static bool IsOnCooldown(CardModel card) => TurnsLeft.Get(card) > 0;

    public static int TurnsLeftOf(CardModel card) => Math.Max(TurnsLeft.Get(card), 0);

    public static void Start(CardModel card, int turns)
    {
        TurnsLeft.Set(card, turns);
        if (!card.Keywords.Contains(FryCardKeywords.Cooldown)) card.AddKeyword(FryCardKeywords.Cooldown);
        if (!card.Keywords.Contains(CardKeyword.Retain))
        {
            card.AddKeyword(CardKeyword.Retain);
            AddedRetain.Set(card, true);
        }
    }

    public static void Tick(CardModel card)
    {
        var left = TurnsLeft.Get(card) - 1;
        if (left > 0) TurnsLeft.Set(card, left);
        else End(card);
    }

    public static void End(CardModel card)
    {
        TurnsLeft.Set(card, 0);
        card.RemoveKeyword(FryCardKeywords.Cooldown);
        if (AddedRetain.Get(card))
        {
            card.RemoveKeyword(CardKeyword.Retain);
            AddedRetain.Set(card, false);
        }
    }
}

/// <summary>
/// Our Mages Are Different: a Seels card played while on its Cooldown does nothing (it's discarded as usual).
/// Patches every Seels card's own OnPlay.
/// </summary>
[HarmonyPatch]
public static class OurMagesCooldownPatch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(SeelsTheRecklessCard).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(SeelsTheRecklessCard)))
            .Select(t => AccessTools.DeclaredMethod(t, "OnPlay"))
            .Where(m => m is not null)
            .Cast<MethodBase>();
    }

    [HarmonyPrefix]
    public static bool Prefix(CardModel __instance, ref Task __result)
    {
        if (!OurMagesCooldown.IsOnCooldown(__instance)) return true;

        OurMagesCooldown.End(__instance);
        __result = Task.CompletedTask;
        return false;
    }
}
