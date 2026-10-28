using MegaCrit.Sts2.Core.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

/// <summary>
/// Stand-in for the epoch properties BaseLib PR #331 adds to <c>CustomCharacterModel</c> (same names, same meaning).
/// Implement it on a character and only declare the epochs it has; the others default to null.
/// The unlock prerequisite is the character's own <c>UnlocksAfterRunAs</c> override, like in the PR:
/// null or Ironclad = unlocked by the Neow epoch (after the very first run), any other character = after a run with them.
/// </summary>
public interface IEpochCharacter
{
    /// <summary>
    /// Unlocks the character. When set, the character is locked until this epoch is revealed.
    /// </summary>
    EpochModel? UnlockEpoch => null;

    /// <summary>
    /// Obtained by beating Act 1 with the character.
    /// </summary>
    EpochModel? Act1Epoch => null;

    /// <summary>
    /// Obtained by beating Act 2 with the character.
    /// </summary>
    EpochModel? Act2Epoch => null;

    /// <summary>
    /// Obtained by beating Act 3 with the character.
    /// </summary>
    EpochModel? Act3Epoch => null;

    /// <summary>
    /// Obtained after defeating 15 elites with the character.
    /// </summary>
    EpochModel? FifteenElitesEpoch => null;

    /// <summary>
    /// Obtained after defeating 15 bosses with the character.
    /// </summary>
    EpochModel? FifteenBossesEpoch => null;

    /// <summary>
    /// Obtained by winning an Ascension 1 run with the character.
    /// </summary>
    EpochModel? AscensionOneEpoch => null;
}
