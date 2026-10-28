using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

public sealed class SeelsStory : CustomStoryModel
{
    public override EpochModel[] Epochs =>
    [
        EpochModel.Get<SeelsUnlockEpoch>(),
    ];
}

/// <summary>
/// Placeholder. Play 1 game as Slal. Unlocks Seels.
/// </summary>
public class SeelsUnlockEpoch : CharacterUnlockEpoch<SeelsTheReckless, SeelsStory>
{
    public override EpochEra Era => EpochEra.Invitation7; //todo : choose placement
    public override int EraPosition => 3;
}
