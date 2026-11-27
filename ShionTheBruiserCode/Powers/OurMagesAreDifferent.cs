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
/// Whenever a Partner plays an Exhaust card that Blood Bends X, it loses Exhaust (goes to the Discard Pile instead).
/// If X ≥ 1, it also gets Cooldown X: it returns to their Hand with Retain and is free for X turns; played during those
/// turns, it's discarded with no effect (OurMagesCooldownPatch).
/// </summary>
public class OurMagesAreDifferent : TeamUpPower, ISeelsListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(FryCardKeywords.Cooldown), HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    private readonly Dictionary<CardModel, int> _pending = [];

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner || delta >= 0 || Owner.IsDead || Owner.Player is not { } player) return;

        Flash();
        await PlayerCmd.GainStars(Amount, player);
    }

    public Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source)
    {
        if (source is null || x < 0 || !LivingPartners.Contains(player.Creature) || OurMagesCooldown.IsOnCooldown(source)) return Task.CompletedTask;
        _pending[source] = x;
        return Task.CompletedTask;
    }

    /// <summary>
    /// A Partner's Exhaust card played while on Cooldown does nothing and is discarded, not Exhausted.
    /// The result pile is picked before OnPlay, while the card is still on Cooldown (the patch ends it during OnPlay).
    /// </summary>
    public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation location)
    {
        if (location.pileType != PileType.Exhaust || !OurMagesCooldown.IsOnCooldown(card)
            || !card.Keywords.Contains(CardKeyword.Exhaust) || !Partners.Contains(card.Owner.Creature)) return location;
        return location with { pileType = PileType.Discard };
    }

    /// <summary>
    /// Runs while the card is still in the Play pile, before the game sends it to its result pile: moving it out of
    /// the Play pile here skips the Exhaust.
    /// </summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!_pending.TryGetValue(card, out var x)) return;
        if (cardPlay.PlayIndex < cardPlay.PlayCount - 1) return;
        _pending.Remove(card);

        // Only Exhaust cards on their way to the Exhaust Pile (not Powers, not cards Exhausted by something else).
        if (cardPlay.ResultPile is not PileType.Exhaust || !card.Keywords.Contains(CardKeyword.Exhaust)) return;
        if (card.Pile?.Type is not PileType.Play) return;

        Flash();
        if (x > 0)
        {
            OurMagesCooldown.Start(card, x);
            await CardPileCmd.Add(card, PileType.Hand);
        }
        else
        {
            await CardPileCmd.Add(card, PileType.Discard);
        }
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
