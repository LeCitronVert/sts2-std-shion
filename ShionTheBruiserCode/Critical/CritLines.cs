using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Critical;

/// <summary>
/// Character-specific speech bubbles on critical hits.
///
/// Lines live in <c>combat_messages.json</c>, keyed by the speaker's character id entry:
/// <c>{CHARACTER}.critLines.{N}</c> and, optionally, <c>{CHARACTER}.critLinesBerserk.{N}</c>
/// (used instead while the speaker is Berserk). N counts up from 0 with no gaps.
/// Characters with no lines never speak on crit. Adding a line only takes a loc entry.
/// </summary>
public static class CritLines
{
    private const string Table = "combat_messages";
    private const float SpeakChance = 0.10f;

    private static readonly Dictionary<string, int> CountCache = new();

    /// <summary>
    /// Rolls the 10% chance and, on success, plays a random crit line for <paramref name="speaker"/>.
    /// Only call from real damage paths (consumes run RNG so every client shows the same line).
    /// </summary>
    public static void TryPlay(Creature speaker, Rng rng)
    {
        var character = speaker.Player?.Character;
        if (character == null) return;

        string prefix = character.Id.Entry + ".critLines";
        if (ShionMechanicsHelper.IsBerserk(speaker) && Count(prefix + "Berserk") > 0)
            prefix += "Berserk";

        int count = Count(prefix);
        if (count == 0) return;

        if (rng.NextFloat() >= SpeakChance) return;

        int index = rng.NextInt(0, count);
        TalkCmd.Play(new LocString(Table, $"{prefix}.{index}"), speaker, VfxColor.Purple, VfxDuration.Short);
    }

    private static int Count(string prefix)
    {
        if (CountCache.TryGetValue(prefix, out int cached)) return cached;

        int n = 0;
        while (LocString.Exists(Table, $"{prefix}.{n}")) n++;
        CountCache[prefix] = n;
        return n;
    }
}
