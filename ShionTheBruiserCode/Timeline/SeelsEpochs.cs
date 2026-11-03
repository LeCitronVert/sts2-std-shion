using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

// Seels' story, from the "Slay the Dreamers - Al notes - Seel - Epochs" sheet.
// Unlock conditions are set on the character (Character/SeelsTheReckless.cs), except Condensed Might's.
// What each epoch unlocks isn't decided yet: add Cards, Relics or Potions (exactly 3 of one kind) later,
// then remove the epoch's placeholder unlockText from epochs.json.

public sealed class SeelsStory : CustomStoryModel
{
    // Chapter order (the sheet's order)
    public override EpochModel[] Epochs =>
    [
        EpochModel.Get<SeelsBoringEpoch>(),
        EpochModel.Get<SeelsUnlockEpoch>(),
        EpochModel.Get<SeelsHoldOnEpoch>(),
        EpochModel.Get<SeelsWaitForMeGuysEpoch>(),
        EpochModel.Get<SeelsWellThisBlowsEpoch>(),
        EpochModel.Get<SeelsRagtagPalsEpoch>(),
        EpochModel.Get<SeelsCondensedMightEpoch>(),
        EpochModel.Get<SeelsRollWithThePunchesEpoch>(),
    ];
}

/// <summary>
/// 1. "Bo-riiiing!" Beat Act 1 with Seels. Column of Shion's "New Challenges".
/// </summary>
public class SeelsBoringEpoch : DreamersEpoch<SeelsStory>
{
    public override EpochEra Era => EpochEra.Invitation2;
    public override int EraPosition => 4;
    protected override string PortraitFile => "seels/boring";
}

/// <summary>
/// 2. "Finally!" Play a run with Slal. Unlocks Seels. Column of "Grand Strategy" (Regent 4).
/// Keeps its placeholder class name so saves that already unlocked Seels keep him.
/// </summary>
public class SeelsUnlockEpoch : CharacterUnlockEpoch<SeelsTheReckless, SeelsStory>
{
    public override EpochEra Era => EpochEra.Invitation3;
    public override int EraPosition => 3;
    protected override string PortraitFile => "seels/finally";
}

/// <summary>
/// 3. "Hold On..." Defeat 15 elites with Seels. Column of "Darv".
/// </summary>
public class SeelsHoldOnEpoch : DreamersEpoch<SeelsStory>
{
    public override EpochEra Era => EpochEra.Invitation4;
    public override int EraPosition => 2;
    protected override string PortraitFile => "seels/hold_on";
}

/// <summary>
/// 4. "Wait For Me, Guys!" Beat Act 2 with Seels. Column of "Life and Death" (Necrobinder 1).
/// </summary>
public class SeelsWaitForMeGuysEpoch : DreamersEpoch<SeelsStory>
{
    public override EpochEra Era => EpochEra.Invitation5;
    public override int EraPosition => 5; // Above Shion's New Friends
    protected override string PortraitFile => "seels/wait_for_me_guys";
}

/// <summary>
/// 5. "Well, This Blows." Beat Act 3 with Seels. Column of "The Daily".
/// </summary>
public class SeelsWellThisBlowsEpoch : DreamersEpoch<SeelsStory>
{
    public override EpochEra Era => EpochEra.Invitation6;
    public override int EraPosition => 6; // Above Slal's Mistake
    protected override string PortraitFile => "seels/well_this_blows";
}

/// <summary>
/// 6. "Ragtag Pals" Defeat 15 bosses with Seels. Column of Shion's "If One Doesn't Cut it...".
/// </summary>
public class SeelsRagtagPalsEpoch : DreamersEpoch<SeelsStory>
{
    public override EpochEra Era => EpochEra.Invitation7;
    public override int EraPosition => 2;
    protected override string PortraitFile => "seels/ragtag_pals";
}

/// <summary>
/// 7. "Condensed Might" Win a run with the Red Chaos Emerald as Seels. Column of "Ragtag Pals".
/// </summary>
public class SeelsCondensedMightEpoch : DreamersEpoch<SeelsStory>
{
    public override EpochEra Era => EpochEra.Invitation7;
    public override int EraPosition => 3;
    protected override string PortraitFile => "seels/condensed_might";

    // Same as Shion's Taste of Power: the combo "Chaos Emeralds" relic counts too
    public override bool IsEarnedPostRun(SerializablePlayer player, SerializableRun run, bool victory) =>
        victory
        && player.CharacterId == ModelDb.Character<SeelsTheReckless>().Id
        && player.Relics.Any(r => r.Id == ModelDb.Relic<RedChaosEmerald>().Id || r.Id == ModelDb.Relic<RedChaosEmeralds>().Id);
}

/// <summary>
/// 8. "Roll With The Punches" Win a run in Ascension 1 with Seels.
/// One column after "If One Doesn't Cut it...": the base game's empty "The End" era.
/// </summary>
public class SeelsRollWithThePunchesEpoch : DreamersEpoch<SeelsStory>
{
    public override EpochEra Era => EpochEra.Peace0;
    public override int EraPosition => 0;
    protected override string PortraitFile => "seels/roll_with_the_punches";
}
