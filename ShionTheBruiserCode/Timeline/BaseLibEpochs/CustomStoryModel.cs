using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Timeline;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

/// <summary>
/// In-mod copy of BaseLib PR #331's <c>BaseLib.Abstracts.CustomStoryModel</c>.
/// Not stored in ModelDb: registered by <see cref="CustomEpochRegistry"/>.
/// </summary>
public abstract class CustomStoryModel : StoryModel
{
    /// <summary>
    /// Must match 1 to 1 with the <see cref="EpochModel.StoryId"/> of its epochs.
    /// Not slugified here: the game slugifies it for lookups.
    /// </summary>
    protected override string Id => $"{GetType().GetPrefix()}{GetType().Name}";

    internal string IdInternal => Id;
}
