using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

public sealed class FryStory : CustomStoryModel
{
    public override EpochModel[] Epochs =>
    [
        EpochModel.Get<FryUnlockEpoch>(),
    ];
}

/// <summary>
/// Placeholder. Play 1 game as Seels. Unlocks Fry.
/// </summary>
public class FryUnlockEpoch : CharacterUnlockEpoch<FryTheCommander, FryStory>
{
    public override EpochEra Era => EpochEra.Invitation7; //todo : choose placement
    public override int EraPosition => 4;
}
