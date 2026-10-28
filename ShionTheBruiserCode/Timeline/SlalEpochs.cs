using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

public sealed class SlalStory : CustomStoryModel
{
    public override EpochModel[] Epochs =>
    [
        EpochModel.Get<SlalUnlockEpoch>(),
    ];
}

/// <summary>
/// Placeholder. Play 1 game as Shion. Unlocks Slal.
/// </summary>
public class SlalUnlockEpoch : CharacterUnlockEpoch<SlalTheEndless, SlalStory>
{
    public override EpochEra Era => EpochEra.Invitation7; //todo : choose placement
    public override int EraPosition => 2;
}
