using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Everything about the Act 4 (Strife) Architect fight that is meant to be tweaked: the summon cycle,
/// the buff cycle, Growing Impatience and the Super Form numbers.
/// </summary>
public static class StrifeConfig
{
    // ── Summon cycle ─────────────────────────────────────────────────────────

    /// <summary>One line of the summon cycle: either fixed encounters summoned together, or an unfought boss of an act.</summary>
    public sealed record SummonEntry(IReadOnlyList<Func<EncounterModel>> Encounters, int? UnfoughtBossOfActIndex = null)
    {
        public static SummonEntry Fixed(params Func<EncounterModel>[] encounters) => new(encounters);

        /// <param name="actIndex">0 = Act 1, 1 = Act 2, 2 = Act 3.</param>
        public static SummonEntry UnfoughtBoss(int actIndex) => new([], actIndex);
    }

    /// <summary>
    /// The Architect summons these in order. Every 2 lines summoned, he prepares his big attack once they're dead.
    /// The fight ends ("defeated") once the whole list was summoned and the last big attack was done.
    /// </summary>
    public static readonly IReadOnlyList<SummonEntry> SummonCycle =
    [
        SummonEntry.Fixed(ModelDb.Encounter<BygoneEffigyElite>, ModelDb.Encounter<ByrdonisElite>),
        SummonEntry.UnfoughtBoss(0),
        SummonEntry.Fixed(ModelDb.Encounter<PhrogParasiteElite>, ModelDb.Encounter<SkulkingColonyElite>),
        SummonEntry.UnfoughtBoss(0),
        SummonEntry.Fixed(ModelDb.Encounter<PhantasmalGardenersElite>, ModelDb.Encounter<TerrorEelElite>),
        SummonEntry.UnfoughtBoss(1),
        SummonEntry.Fixed(ModelDb.Encounter<DecimillipedeElite>, ModelDb.Encounter<EntomancerElite>),
        SummonEntry.UnfoughtBoss(1),
        SummonEntry.Fixed(ModelDb.Encounter<InfestedPrismsElite>, ModelDb.Encounter<KnightsElite>),
        SummonEntry.UnfoughtBoss(2),
        SummonEntry.Fixed(ModelDb.Encounter<MechaKnightElite>, ModelDb.Encounter<SoulNexusElite>),
        SummonEntry.UnfoughtBoss(2),
    ];

    /// <summary>How many summon lines before each big attack.</summary>
    public const int SummonsPerBigAttack = 2;

    /// <summary>
    /// Bosses the Architect never summons. Kaiser Crab's body is part of its own boss background (Crusher and Rocket look
    /// it up there), so it can't exist in the Architect's room: it hangs and crashes.
    /// </summary>
    public static readonly IReadOnlySet<Type> UnsummonableBosses = new HashSet<Type> { typeof(KaiserCrabBoss) };

    public static bool CanBeSummoned(EncounterModel encounter) => !UnsummonableBosses.Contains(encounter.GetType());

    /// <summary>Boss pools by act index, all act variants of that index (used when the visited act has no unfought boss left).</summary>
    public static IEnumerable<ActModel> ActsOfIndex(int actIndex) => actIndex switch
    {
        0 => [ModelDb.Act<Overgrowth>(), ModelDb.Act<Underdocks>()],
        1 => [ModelDb.Act<Hive>()],
        _ => [ModelDb.Act<Glory>()],
    };

    // ── Rings ────────────────────────────────────────────────────────────────

    // ── Perfected Design (every summon) ──────────────────────────────────────

    /// <summary>His summons deal this much more damage (×) and take this much of the damage (×).</summary>
    public const decimal PerfectedDesignDamageDealt = 2m;
    public const decimal PerfectedDesignDamageTaken = 0.5m;

    // His summons have no Ring Bounty since 2026-11-26: whoever damages the Architect gains that many Rings instead
    // (ArchitectsDesign). The RingBounty power is kept, unused.

    // ── Buff cycle (while summons are alive) ─────────────────────────────────

    public enum BuffKind
    {
        Strength,
        Block,
        Vigor,
        HealPercent,
        /// <summary>Every following buff gets stronger (one more tier).</summary>
        Intensify,
        /// <summary>Growing Impatience: one less card per turn.</summary>
        Impatience,
    }

    /// <summary>Since 2026-11-27 (harder Architect): three Intensify per 10 buffs instead of one, the first on his second buff.</summary>
    public static readonly IReadOnlyList<BuffKind> BuffCycle =
    [
        BuffKind.Strength, BuffKind.Intensify, BuffKind.Block, BuffKind.Vigor, BuffKind.Intensify,
        BuffKind.Strength, BuffKind.HealPercent, BuffKind.Intensify, BuffKind.Vigor, BuffKind.Impatience,
    ];

    /// <summary>Tier his buffs start at (each tier doubles them, see <see cref="BuffValue"/>). 1 = his first buffs are already doubled.</summary>
    public const int StartingTier = 1;

    /// <summary>Value of a buff at tier 0 (before any Intensify). Each Intensify doubles it.</summary>
    public static int BuffBaseValue(BuffKind kind) => kind switch
    {
        BuffKind.Strength => 2,
        BuffKind.Block => 20,
        BuffKind.Vigor => 7,
        BuffKind.HealPercent => 5,
        _ => 0,
    };

    /// <summary>The Block buff doesn't double: each Intensify adds this much Plating to it instead.</summary>
    public const int PlatingPerTier = 10;

    /// <summary>The heal buff never heals more than this % of Max HP.</summary>
    public const int MaxHealPercent = 50;

    public static int BuffValue(BuffKind kind, int tier) => kind switch
    {
        BuffKind.Block => BuffBaseValue(kind),
        BuffKind.HealPercent => Math.Min(MaxHealPercent, BuffBaseValue(kind) << Math.Min(tier, 16)),
        _ => BuffBaseValue(kind) << Math.Min(tier, 16),
    };

    public static int PlatingValue(int tier) => PlatingPerTier * tier;

    // ── Make Your Point (DPS checks) ─────────────────────────────────────────

    /// <summary>Damage of his attack (total, split between the hits). Ascension 9 (Deadly Enemies) uses the second one.</summary>
    public const int CheckAttackDamage = 150;
    public const int CheckAttackDamageAscension = 225;

    /// <summary>Each consecutive turn he attacks in the same check, the damage is multiplied by (1 + this).</summary>
    public const decimal ConsecutiveAttackBoost = 0.5m;

    /// <summary>Each attack he made makes the later checks deal (this) more damage (additive) and one more hit.</summary>
    public const decimal AttackCycleDamageBoost = 0.2m;
    public const int AttackCycleExtraHits = 1;

    /// <summary>
    /// Health bar of a check: HpBase × players (dead ones too) × (1 + PerExtraPlayer × (players − 1)) × (1 + PerAttack × attacks
    /// he made). Ascension 8 (Tough Enemies) uses the second base. With 0.5 per extra player: ×1 solo, ×3 for 2 players,
    /// ×6 for 3, ×10 for 4.
    /// </summary>
    public const int CheckHpBase = 450;
    public const int CheckHpBaseAscension = 600;
    public const decimal CheckHpPerExtraPlayer = 0.5m;
    public const decimal CheckHpPerAttack = 0.2m;

    // ── Growing Impatience ───────────────────────────────────────────────────

    /// <summary>Growing Impatience: after playing this many cards in a turn, your turn ends.</summary>
    public const int StartingCardLimit = 10;
    public const int MinimumCardLimit = 1;

    // ── Architect lines ──────────────────────────────────────────────────────

    /// <summary>Number of "BIG_ATTACK_LINE.N" loc entries (he says one before preparing his big attack).</summary>
    public const int BigAttackLineCount = 8;

    // ── Super Form ───────────────────────────────────────────────────────────

}
