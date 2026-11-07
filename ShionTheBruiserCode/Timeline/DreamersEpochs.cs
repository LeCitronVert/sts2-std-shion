using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

// The Dreamers' shared story, from the "Slay the Dreamers - Al notes - Dreamers - Epochs" sheet.
// Its slot opens with Shion's "The Call" (ShionTheCallEpoch.GetTimelineExpansion): the Chaos Emeralds it needs
// only exist once the Dreamers are around.

public sealed class DreamersStory : CustomStoryModel
{
    public override EpochModel[] Epochs =>
    [
        EpochModel.Get<WarmEmbraceEpoch>(),
    ];
}

/// <summary>
/// 1. "Warm Embrace" Unlock two Chaos Emeralds (in the save file). Column of Seels' "Condensed Might".
/// Unlocks Cosmo, who sometimes welcomes the party instead of Neow (Ancients/CosmoSpawn.cs).
/// </summary>
public class WarmEmbraceEpoch : DreamersEpoch<DreamersStory>
{
    public const int EmeraldsNeeded = 2;

    public override EpochEra Era => EpochEra.Invitation7;
    public override int EraPosition => 5;

    // Checked after ChaosEmeraldUnlockPatch has saved this run's Emeralds
    public override bool IsEarnedPostRun(SerializablePlayer player, SerializableRun run, bool victory) =>
        ChaosEmeraldHelper.UnlockedColors.Count >= EmeraldsNeeded;
}
