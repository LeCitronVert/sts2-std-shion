using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

/// <summary>
/// The epoch that unlocks <typeparamref name="TCharacter"/> (set it as the character's UnlockEpoch).
/// Revealing it unlocks the character and reveals the slots of the rest of <typeparamref name="TStory"/>,
/// plus the unlock epochs of characters whose UnlocksAfterRunAs is <typeparamref name="TCharacter"/>
/// (BaseLib's PR only does that for base characters).
/// Its text needs an unlockText key in epochs.json, like the base character unlock epochs.
/// </summary>
public abstract class CharacterUnlockEpoch<TCharacter, TStory> : DreamersEpoch<TStory>
    where TCharacter : CharacterModel
    where TStory : CustomStoryModel, new()
{
    public override EpochModel[] GetTimelineExpansion() =>
    [
        .. new TStory().Epochs.Where(e => e.Id != Id),
        .. ModelDb.AllCharacters
            .Where(c => EpochProgress.UnlocksAfterRunAs(c) is TCharacter)
            .OfType<IEpochCharacter>()
            .Select(c => c.UnlockEpoch)
            .OfType<EpochModel>(),
    ];

    public override void QueueUnlocks()
    {
        NTimelineScreen.Instance.QueueCharacterUnlock<TCharacter>(this);
        SaveManager.Instance.Progress.PendingCharacterUnlock = ModelDb.Character<TCharacter>().Id;
        QueueTimelineExpansion(GetTimelineExpansion());
    }
}
