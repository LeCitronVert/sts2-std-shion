using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Potions;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

// Shion's story, from the "Slay the Dreamers - Shion - Epochs" sheet.
// Unlock conditions are set on the character (Character/ShionTheBruiser.cs), except Taste of Power's.

public sealed class ShionStory : CustomStoryModel
{
    // Chapter order
    public override EpochModel[] Epochs =>
    [
        EpochModel.Get<ShionTheCallEpoch>(),
        EpochModel.Get<ShionNewChallengesEpoch>(),
        EpochModel.Get<ShionMemoriesEpoch>(),
        EpochModel.Get<ShionNewFriendsEpoch>(),
        EpochModel.Get<ShionDeterminationEpoch>(),
        EpochModel.Get<ShionFamiliarSceneEpoch>(),
        EpochModel.Get<ShionTasteOfPowerEpoch>(),
        EpochModel.Get<ShionIfOneDoesntCutItEpoch>(),
    ];
}

/// <summary>
/// 1. Play 1 game as the Silent. Unlocks Shion. Column of "I Arrive" (Regent 2).
/// </summary>
public class ShionTheCallEpoch : CharacterUnlockEpoch<Character.ShionTheBruiser, ShionStory>
{
    public override EpochEra Era => EpochEra.Invitation1;
    public override int EraPosition => 5; // Rows 0-4 are taken: this column gets taller than the others
    protected override string PortraitFile => "shion/the_call";
}

/// <summary>
/// 2. Complete Act 1 with Shion. Column of "We Need a Hero!" (Regent 3).
/// </summary>
public class ShionNewChallengesEpoch : DreamersEpoch<ShionStory>
{
    public override EpochEra Era => EpochEra.Invitation2;
    public override int EraPosition => 3;
    protected override string PortraitFile => "shion/new_challenges";

    public override IReadOnlyList<PotionModel> Potions =>
    [
        ModelDb.Potion<CriticalPotion>(),
        ModelDb.Potion<DraconicJuice>(),
        ModelDb.Potion<UltraSpicyBrew>(),
    ];
}

/// <summary>
/// 3. Defeat 15 elites with Shion. Column of "Grand Strategy" (Regent 4).
/// </summary>
public class ShionMemoriesEpoch : DreamersEpoch<ShionStory>
{
    public override EpochEra Era => EpochEra.Invitation3;
    public override int EraPosition => 2;
    protected override string PortraitFile => "shion/memories";

    public override IReadOnlyList<CardModel> Cards =>
    [
        ModelDb.Card<ShionRageGuard>(),
        ModelDb.Card<ShionRageCharge>(),
        ModelDb.Card<ShionRageStrike>(),
    ];
}

/// <summary>
/// 4. Complete Act 2 with Shion. Column of "Soup" (Silent 7).
/// </summary>
public class ShionNewFriendsEpoch : DreamersEpoch<ShionStory>
{
    public override EpochEra Era => EpochEra.Invitation5;
    public override int EraPosition => 4;
    protected override string PortraitFile => "shion/new_friends";

    public override IReadOnlyList<CardModel> Cards =>
    [
        ModelDb.Card<ShionChildOfMany>(),
        ModelDb.Card<ShionMonstermancy>(),
        ModelDb.Card<ShionExtremeSpeed>(),
    ];
}

/// <summary>
/// 5. Defeat 15 bosses with Shion. Column of "The Daily".
/// </summary>
public class ShionDeterminationEpoch : DreamersEpoch<ShionStory>
{
    public override EpochEra Era => EpochEra.Invitation6;
    public override int EraPosition => 2;
    protected override string PortraitFile => "shion/determination";

    public override IReadOnlyList<RelicModel> Relics =>
    [
        ModelDb.Relic<AbbyTheAbsol>(),
        ModelDb.Relic<EdgeTheChao>(),
        ModelDb.Relic<CommanderCap>(),
    ];
}

/// <summary>
/// 6. Complete Act 3 with Shion. Column of "We Need a Hero!" (Regent 3).
/// </summary>
public class ShionFamiliarSceneEpoch : DreamersEpoch<ShionStory>
{
    public override EpochEra Era => EpochEra.Invitation2;
    public override int EraPosition => 4;
    protected override string PortraitFile => "shion/a_familiar_scene";

    public override IReadOnlyList<CardModel> Cards =>
    [
        ModelDb.Card<ShionSpiritBurst>(),
        ModelDb.Card<ShionBreakingSwipes>(),
        ModelDb.Card<ShionRevengeDrive>(),
    ];
}

/// <summary>
/// 7. Win a run with the Purple Chaos Emerald as Shion. Column of "Nemesis" (Silent 6).
/// </summary>
public class ShionTasteOfPowerEpoch : DreamersEpoch<ShionStory>
{
    public override EpochEra Era => EpochEra.Prehistoria2;
    public override int EraPosition => 2;
    protected override string PortraitFile => "shion/a_taste_of_power";

    public override IReadOnlyList<CardModel> Cards =>
    [
        ModelDb.Card<ShionFeint>(),
        ModelDb.Card<ShionEleventhHour>(),
        ModelDb.Card<ShionBerserkForm>(),
    ];

    // The combo "Chaos Emeralds" relic has the same effect as the solo one, so it counts too
    public override bool IsEarnedPostRun(SerializablePlayer player, SerializableRun run, bool victory) =>
        victory
        && player.CharacterId == ModelDb.Character<Character.ShionTheBruiser>().Id
        && player.Relics.Any(r => r.Id == ModelDb.Relic<PurpleChaosEmerald>().Id || r.Id == ModelDb.Relic<PurpleChaosEmeralds>().Id);
}

/// <summary>
/// 8. Win a run in Ascension 1 with Shion. Column of "To Kill" (Necrobinder 7).
/// </summary>
public class ShionIfOneDoesntCutItEpoch : DreamersEpoch<ShionStory>
{
    public override EpochEra Era => EpochEra.Invitation7;
    public override int EraPosition => 0;
    protected override string PortraitFile => "shion/if_one_doesnt_cut_it";

    public override IReadOnlyList<CardModel> Cards =>
    [
        ModelDb.Card<ShionFaeReturn>(),
        ModelDb.Card<ShionUma>(),
        ModelDb.Card<ShionDragonsong>(),
    ];
}
