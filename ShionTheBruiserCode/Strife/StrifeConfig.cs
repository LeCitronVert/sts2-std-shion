using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rooms;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Everything about the Act 4 (Strife) Architect fight that is meant to be tweaked: the summon cycle, the Ring Bounties,
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

    // ── Ring Bounty ──────────────────────────────────────────────────────────

    public const int EliteBounty = 50;
    public const int BossBounty = 100;

    /// <summary>
    /// Creatures spawned mid-fight by a summon (Phrog Parasite's Wrigglers, Fabricator's bots...) that aren't listed in
    /// <see cref="BountyByMonster"/>.
    /// </summary>
    public const int SpawnedBounty = 0; //todo : choose bounty

    /// <summary>
    /// Per-monster Ring Bounty, overriding the Elite / Boss / spawned default. Keyed by the monster's class.
    /// Example: [typeof(Wriggler)] = 5.
    /// </summary>
    public static readonly IReadOnlyDictionary<Type, int> BountyByMonster = new Dictionary<Type, int>
    {
    };

    /// <param name="monster">The creature's monster.</param>
    /// <param name="summonedAs">Room type of the encounter it was summoned with, or null for a creature spawned mid-fight.</param>
    public static int BountyFor(MonsterModel monster, RoomType? summonedAs)
    {
        if (BountyByMonster.TryGetValue(monster.GetType(), out var bounty)) return bounty;
        return summonedAs switch
        {
            RoomType.Boss => BossBounty,
            RoomType.Elite => EliteBounty,
            _ => SpawnedBounty,
        };
    }

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

    public static readonly IReadOnlyList<BuffKind> BuffCycle =
    [
        BuffKind.Strength, BuffKind.Block, BuffKind.Vigor, BuffKind.HealPercent, BuffKind.Intensify,
        BuffKind.Strength, BuffKind.Block, BuffKind.Vigor, BuffKind.HealPercent, BuffKind.Impatience,
    ];

    /// <summary>Value of a buff at tier 0 (before any Intensify), and how much each Intensify adds.</summary>
    public static (int Base, int PerTier) BuffValues(BuffKind kind) => kind switch
    {
        BuffKind.Strength => (1, 1),
        BuffKind.Block => (10, 2),
        BuffKind.Vigor => (5, 2),
        BuffKind.HealPercent => (10, 2),
        _ => (0, 0),
    };

    public static int BuffValue(BuffKind kind, int tier)
    {
        var (b, perTier) = BuffValues(kind);
        return b + perTier * tier;
    }

    // ── Growing Impatience ───────────────────────────────────────────────────

    /// <summary>Growing Impatience: after playing more than this many cards in a turn, your turn ends.</summary>
    public const int StartingCardLimit = 10;
    public const int MinimumCardLimit = 1;

    // ── Architect lines ──────────────────────────────────────────────────────

    /// <summary>Number of "BIG_ATTACK_LINE.N" loc entries (he says one before preparing his big attack).</summary>
    public const int BigAttackLineCount = 8;

    // ── Super Form ───────────────────────────────────────────────────────────

    /// <summary>Gold gained at the Act 4 start (shadowed Chaos Emeralds node).</summary>
    public const int ShrineGold = 100;
}
