using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

/// <summary>
/// Monster Spell's speech bubbles: when played, 50% chance to say a line depending on the kind of enemy
/// the spell was copied from, and on whether the speaker is Berserk.
///
/// Lines live in <c>combat_messages.json</c>: <c>MONSTER_SPELL.{Group}.{N}</c> and, while Berserk,
/// <c>MONSTER_SPELL.{Group}Berserk.{N}</c> instead. N counts up from 0 with no gaps.
/// Enemies are grouped by their model's class name; any enemy not listed (including modded ones) uses <c>Other</c>.
/// </summary>
public static class MonsterSpellLines
{
    private const string Table = "combat_messages";
    private const string Prefix = "MONSTER_SPELL";
    private const float SpeakChance = 0.5f;
    private const string OtherGroup = "Other";

    private static readonly Dictionary<string, int> CountCache = new();

    private static readonly Dictionary<string, string[]> Groups = new()
    {
        ["Beast"] =
        [
            "Nibbit", "Mawler", "SlitheringStrangler", "VineShambler", "FossilStalker", "TwoTailedRat", "SpinyToad",
            "TheObscura", "FrogKnight", "Parafright", "Toadpole", "OwlMagistrate", "Byrdonis", "PhantasmalGardener",
            "TerrorEel", "CeremonialBeast", "TestSubject",
        ],
        ["Slime"] =
        [
            "LeafSlimeM", "LeafSlimeS", "TwigSlimeM", "TwigSlimeS", "Inklet", "CorpseSlug", "SludgeSpinner", "Seapunk",
            "LivingFog", "GasBomb", "SewerClam", "Ovicopter", "ToughEgg", "Myte", "LouseProgenitor", "SlumberingBeetle",
            "SlimedBerserker", "PhrogParasite", "SkulkingColony", "Entomancer", "Vantom", "WaterfallGiant", "SoulFysh",
            // Kaiser Crab's two halves
            "Crusher", "Rocket",
        ],
        ["Bug"] =
        [
            "ShrinkerBeetle", "FuzzyWurmCrawler", "Flyconid", "Fogmog", "EyeWithTeeth", "SnappingJaxfruit",
            "BowlbugEgg", "BowlbugNectar", "BowlbugRock", "BowlbugSilk", "Exoskeleton", "ThievingHopper", "Tunneler",
            "HunterKiller", "ScrollOfBiting", "DecimillipedeSegment", "DecimillipedeSegmentBack",
            "DecimillipedeSegmentFront", "DecimillipedeSegmentMiddle", "LagavulinMatriarch", "TheInsatiable",
        ],
        ["Machine"] =
        [
            "CubexConstruct", "PunchConstruct", "Chomper", "GlobeHead", "LivingShield", "Axebot", "Fabricator",
            "Guardbot", "Noisebot", "Stabbot", "MechaKnight",
            // The list had Stabbot twice; Zapbot is the only other bot  //todo : check Zapbot's group
            "Zapbot",
        ],
        ["Silent"] =
        [
            "TheLost", "TheForgotten", "SoulNexus", "InfestedPrism", "BygoneEffigy", "KnowledgeDemon", "Aeonglass",
        ],
        ["Raider"] =
        [
            "AxeRubyRaider", "AssassinRubyRaider", "BruteRubyRaider", "CrossbowRubyRaider", "TrackerRubyRaider",
            "TurretOperator", "MysteriousKnight", "FlailKnight", "SpectralKnight", "MagiKnight", "Queen",
        ],
        ["Cultist"] =
        [
            "CalcifiedCultist", "DampCultist", "DevotedSculptor", "KinPriest", "KinFollower",
        ],
    };

    private static readonly Dictionary<string, string> GroupByMonster = Groups
        .SelectMany(g => g.Value.Select(monster => (monster, g.Key)))
        .ToDictionary(p => p.monster, p => p.Key);

    /// <summary>Identifier stored by Monster Spell for the enemy it was copied from (its model's class name).</summary>
    public static string? SourceId(Creature monster) => monster.Monster?.GetType().Name;

    public static string GroupOf(string? sourceId) =>
        sourceId != null && GroupByMonster.TryGetValue(sourceId, out var group) ? group : OtherGroup;

    /// <summary>
    /// Rolls the 50% chance and, on success, plays a random line for the enemy group.
    /// Only call from OnPlay (consumes run RNG so every client shows the same line).
    /// </summary>
    public static void TryPlay(Creature speaker, string? sourceId, Rng rng)
    {
        var key = $"{Prefix}.{GroupOf(sourceId)}";
        if (ShionMechanicsHelper.IsBerserk(speaker)) key += "Berserk";

        // Always the same two rolls, whatever the line count: clients with another language (which may
        // have fewer lines) must consume the shared RNG the same way
        var speaks = rng.NextFloat() < SpeakChance;
        var roll = rng.NextInt(0, 10000);

        var count = Count(key);
        if (!speaks || count == 0) return;

        var index = roll % count;
        TalkCmd.Play(new LocString(Table, $"{key}.{index}"), speaker, VfxColor.Purple, VfxDuration.Short);
    }

    private static int Count(string prefix)
    {
        if (CountCache.TryGetValue(prefix, out var cached)) return cached;

        var n = 0;
        while (LocString.Exists(Table, $"{prefix}.{n}")) n++;
        CountCache[prefix] = n;
        return n;
    }
}
