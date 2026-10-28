using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

/// <summary>
/// Base of every epoch of the mod. An epoch only declares data:
///  - where it sits: <c>Era</c> (the column) and <c>EraPosition</c> (the row, 0 = bottom). Rows must be free, see <see cref="EpochValidator"/>.
///  - its art: <see cref="PortraitFile"/>, under images/timeline/ (+ a 250x190 "_packed" thumbnail for the timeline slot).
///  - what it unlocks: <see cref="Cards"/>, <see cref="Relics"/> or <see cref="Potions"/>, exactly 3 of one kind.
///    That content is locked in its pool until the epoch is revealed (<see cref="EpochLocks"/>).
/// Its text lives in localization/*/epochs.json under its Id (title, description, unlockInfo).
/// What obtains it is the character's IEpochCharacter slot, or <see cref="IsEarnedPostRun"/> for anything else.
/// </summary>
public abstract class DreamersEpoch : CustomEpochModel
{
    /// <summary>
    /// Path under images/timeline/, without extension (e.g. "shion/the_call"). Null = placeholder art.
    /// </summary>
    protected virtual string? PortraitFile => null;

    private static string PlaceholderPortraitPath => "char_select_char_name.png".CharacterUiPath();

    public override string CustomRealPortraitPath =>
        PortraitFile is null ? PlaceholderPortraitPath : $"timeline/{PortraitFile}.png".ImagePath();

    public override string CustomPackedPortraitPath =>
        PortraitFile is null ? PlaceholderPortraitPath : $"timeline/{PortraitFile}_packed.png".ImagePath();

    public virtual IReadOnlyList<CardModel> Cards => [];
    public virtual IReadOnlyList<RelicModel> Relics => [];
    public virtual IReadOnlyList<PotionModel> Potions => [];

    protected override bool UnlocksCards => Cards.Count > 0;

    /// <summary>
    /// For unlock conditions the base game doesn't have. Checked at the end of every run (won or lost)
    /// for the local player, after the base checks.
    /// </summary>
    public virtual bool IsEarnedPostRun(SerializablePlayer player, SerializableRun run, bool victory) => false;

    public override string UnlockText =>
        Cards.Count > 0 ? CreateCardUnlockText(Cards.ToList())
        : Relics.Count > 0 ? CreateRelicUnlockText(Relics.ToList())
        : Potions.Count > 0 ? CreatePotionUnlockText(Potions.ToList())
        : base.UnlockText;

    public override void QueueUnlocks()
    {
        if (Cards.Count > 0) NTimelineScreen.Instance.QueueCardUnlock(Cards);
        if (Relics.Count > 0) NTimelineScreen.Instance.QueueRelicUnlock(Relics.ToList());
        if (Potions.Count > 0) NTimelineScreen.Instance.QueuePotionUnlock(Potions.ToList());
    }

    private static List<DreamersEpoch>? _all;

    /// <summary>
    /// One instance of every epoch of the mod.
    /// </summary>
    public static IReadOnlyList<DreamersEpoch> All => _all ??= typeof(DreamersEpoch).Assembly.GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(DreamersEpoch)))
        .Select(t => (DreamersEpoch)Activator.CreateInstance(t)!)
        .ToList();
}

/// <summary>
/// An epoch that is a chapter of <typeparamref name="TStory"/>.
/// </summary>
public abstract class DreamersEpoch<TStory> : DreamersEpoch where TStory : CustomStoryModel
{
    // Same formula as CustomStoryModel.Id, which is protected
    public override string StoryId => $"{typeof(TStory).GetPrefix()}{typeof(TStory).Name}";
}
