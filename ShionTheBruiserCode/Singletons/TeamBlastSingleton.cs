using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Singletons;

/// <summary>
/// Drives the Team Blast gauge, shared by the whole party.
/// <list type="bullet">
/// <item>Team Blast cards are pulled out of the Draw Pile when combat is set up (see
/// <see cref="Patches.TeamBlastHiddenPilePatch"/>) and kept in a hidden pile here.</item>
/// <item>If any player had one, the gauge is active for this combat. Allies fill it by dealing, blocking and taking
/// damage, and whenever any creature dies.</item>
/// <item>Once full, each player with a hidden Team Blast gets an Ethereal copy of it at the start of every turn.
/// Duplicates only give one copy, but each extra one makes the whole team fill the gauge
/// <see cref="DuplicateChargeBonus"/> faster.</item>
/// <item>Playing a Team Blast locks the gauge for the rest of the combat; it then goes back to 0 when combat ends.</item>
/// </list>
/// The gauge value is saved with the run, on every player (all clients keep the same value).
/// </summary>
public class TeamBlastSingleton : CustomSingletonModel
{
    public static TeamBlastSingleton? Instance { get; private set; }

    public TeamBlastSingleton() : base(HookType.Combat)
    {
        Instance = this;
    }

    // ── Balancing ────────────────────────────────────────────────────────────

    public const int GaugePerPlayer = 200;
    public const decimal DamageDealtMultiplier = 1m;
    public const decimal DamageBlockedMultiplier = 1m;
    public const decimal DamageTakenMultiplier = 1m;
    public const decimal DeathBonus = 10m;
    public const decimal AllyOstyDeathMultiplier = 0.5m;
    /// <summary>Gauge fill bonus for each duplicate of a Team Blast a player has (copies after the first).</summary>
    public const decimal DuplicateChargeBonus = 0.2m;

    // ── State ────────────────────────────────────────────────────────────────

    /// <summary>Gauge value, saved with the run so it carries over between combats.</summary>
    public static readonly SavedSpireField<Player, int> SavedGauge = new(() => 0, "TeamBlastGauge");

    /// <summary>Team Blast cards kept out of the combat piles, per player.</summary>
    private readonly Dictionary<Player, List<CardModel>> _hiddenPile = new();

    /// <summary>The Ethereal copies this singleton added. Any other Team Blast entering combat is removed.</summary>
    private readonly HashSet<CardModel> _allowedCopies = new();

    private ICombatState? _combatState;

    /// <summary>The Team Blast that locked the gauge; it still has to finish being played.</summary>
    private CardModel? _usedTeamBlast;

    /// <summary>Gauge gains below 1 point (from multipliers) carry over to the next gain.</summary>
    private decimal _remainder;

    /// <summary>Whether someone had a Team Blast at the start of this combat. The gauge only shows and fills then.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Number of duplicate Team Blasts (per player and card, copies after the first) in this combat.</summary>
    public int DuplicateCount { get; private set; }

    /// <summary>Multiplier on every gauge gain, from <see cref="DuplicateCount"/>.</summary>
    public decimal ChargeMultiplier => 1m + DuplicateChargeBonus * DuplicateCount;

    /// <summary>A Team Blast was used this combat.</summary>
    public bool IsLocked { get; private set; }

    public int MaxValue => GaugePerPlayer * (_combatState?.Players.Count ?? 1);

    public int Value => _combatState?.Players.Select(p => SavedGauge.Get(p)).DefaultIfEmpty(0).Max() ?? 0;

    public bool IsFull => Value >= MaxValue;

    public static bool IsTeamBlast(CardModel card) => card.Keywords.Contains(DreamersKeywords.TeamBlast);

    // ── Hidden pile ──────────────────────────────────────────────────────────

    /// <summary>
    /// Called right after a player's Draw Pile is filled from their deck, before any hook runs.
    /// Removes their Team Blasts from it.
    /// </summary>
    public void HideTeamBlasts(Player player, ICombatState combatState)
    {
        if (_combatState != combatState)
        {
            _combatState = combatState;
            _hiddenPile.Clear();
            _allowedCopies.Clear();
            _remainder = 0m;
            _usedTeamBlast = null;
            IsLocked = false;
            DuplicateCount = 0;
        }

        var drawPile = PileType.Draw.GetPile(player);
        var teamBlasts = drawPile.Cards.Where(IsTeamBlast).ToList();
        foreach (var card in teamBlasts)
        {
            card.RemoveFromCurrentPile(true);
        }

        _hiddenPile[player] = teamBlasts;
        IsActive = _hiddenPile.Values.Any(cards => cards.Count > 0);
        DuplicateCount = _hiddenPile.Values.Sum(cards => cards.Count - DistinctTeamBlasts(cards).Count());
    }

    // ── Gauge ────────────────────────────────────────────────────────────────

    private bool CanFill => IsActive && !IsLocked && _combatState != null && CombatManager.Instance.IsInProgress;

    private void Fill(decimal amount)
    {
        if (!CanFill || amount <= 0 || _combatState == null) return;

        _remainder += amount * ChargeMultiplier;
        var points = (int)Math.Floor(_remainder);
        if (points <= 0) return;
        _remainder -= points;

        var newValue = Math.Min(MaxValue, Value + points);
        if (newValue >= MaxValue) _remainder = 0m;
        SetGauge(_combatState, newValue);
    }

    private static void SetGauge(ICombatState combatState, int value)
    {
        foreach (var player in combatState.Players)
        {
            SavedGauge.Set(player, value);
        }
    }

    /// <summary>Players and their pets.</summary>
    private static bool IsAlly(Creature creature) => creature.Side == CombatSide.Player;

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // HP actually lost, overkill excluded
        var hpDamage = result.UnblockedDamage;
        decimal amount = 0m;

        if (IsAlly(target))
        {
            amount += result.BlockedDamage * DamageBlockedMultiplier;
            amount += hpDamage * DamageTakenMultiplier;
        }
        else if (dealer != null && IsAlly(dealer))
        {
            // Damage dealt to an enemy's Block counts too
            amount += (hpDamage + result.BlockedDamage) * DamageDealtMultiplier;
        }

        Fill(amount);
        return Task.CompletedTask;
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        var isAllyOsty = creature.Monster is Osty && IsAlly(creature);
        Fill(DeathBonus * (isAllyOsty ? AllyOstyDeathMultiplier : 1m));
        return Task.CompletedTask;
    }

    // ── Team Blast copies ────────────────────────────────────────────────────

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!IsActive || IsLocked || !IsFull) return;
        if (!_hiddenPile.TryGetValue(player, out var hidden)) return;

        foreach (var original in DistinctTeamBlasts(hidden))
        {
            var copy = original.CreateClone();
            copy.AddKeyword(CardKeyword.Ethereal);
            _allowedCopies.Add(copy);
            await CardPileCmd.Add(copy, PileType.Hand);
        }
    }

    /// <summary>One card per Team Blast, the upgraded one when there are duplicates.</summary>
    private static IEnumerable<CardModel> DistinctTeamBlasts(IEnumerable<CardModel> cards) =>
        cards.GroupBy(c => c.Id).Select(g => g.OrderByDescending(c => c.IsUpgraded).First());

    /// <summary>Team Blasts can't be generated in combat by any means (clones, gifts…), only by the gauge.</summary>
    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (!IsTeamBlast(card) || _allowedCopies.Contains(card) || card.Pile == null) return;
        await CardPileCmd.RemoveFromCombat(card);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (IsTeamBlast(cardPlay.Card) && !IsLocked)
        {
            IsLocked = true;
            _usedTeamBlast = cardPlay.Card;
            // Don't let it come back from the Discard Pile
            cardPlay.Card.ExhaustOnNextPlay = true;
        }
        return Task.CompletedTask;
    }

    /// <summary>Only one Team Blast per combat.</summary>
    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        return !IsLocked || !IsTeamBlast(card) || card == _usedTeamBlast;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (IsLocked && _combatState != null)
        {
            SetGauge(_combatState, 0);
        }

        IsActive = false;
        IsLocked = false;
        _usedTeamBlast = null;
        _hiddenPile.Clear();
        _allowedCopies.Clear();
        _remainder = 0m;
        DuplicateCount = 0;
        return Task.CompletedTask;
    }

    // ── UI ───────────────────────────────────────────────────────────────────

    public HoverTip CreateGaugeHoverTip()
    {
        var description = new LocString("static_hover_tips", "SHIONTHEBRUISER-TEAM_BLAST_GAUGE.description");
        description.Add("Locked", IsLocked ? 1 : 0);
        description.Add("ChargeBonus", (int)Math.Round(DuplicateChargeBonus * DuplicateCount * 100m));
        return new HoverTip(new LocString("static_hover_tips", "SHIONTHEBRUISER-TEAM_BLAST_GAUGE.title"), description.GetFormattedText());
    }
}
