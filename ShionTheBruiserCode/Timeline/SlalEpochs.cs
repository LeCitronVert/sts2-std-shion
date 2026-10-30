using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Potions;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

// Slal's story, from the "Slay the Dreamers - Slal - Epochs" sheet.
// Unlock conditions are set on the character (Character/SlalTheEndless.cs), except Mistake's.

public sealed class SlalStory : CustomStoryModel
{
    // Chapter order
    public override EpochModel[] Epochs =>
    [
        EpochModel.Get<SlalTheFactoryEpoch>(),
        EpochModel.Get<SlalRejectionsEpoch>(),
        EpochModel.Get<SlalMimickeryEpoch>(),
        EpochModel.Get<SlalCreativeSoulEpoch>(),
        EpochModel.Get<SlalInfiniteWorldsEpoch>(),
        EpochModel.Get<SlalSelfDiscoveryEpoch>(),
        EpochModel.Get<SlalReconnaissanceEpoch>(),
        EpochModel.Get<SlalMistakeEpoch>(),
    ];
}

/// <summary>
/// 1. Complete Act 1 with Slal. Column of "Nemesis" (Silent 6).
/// </summary>
public class SlalTheFactoryEpoch : DreamersEpoch<SlalStory>
{
    public override EpochEra Era => EpochEra.Prehistoria2;
    public override int EraPosition => 2;
    protected override string PortraitFile => "slal/the_factory";

    public override IReadOnlyList<CardModel> Cards =>
    [
        ModelDb.Card<SlalFrontflipAndAHalf>(),
        ModelDb.Card<SlalLaughablePunches>(),
        ModelDb.Card<SlalIntroduction>(),
    ];
}

/// <summary>
/// 2. Complete Act 3 with Slal. Column of "Nemesis" (Silent 6).
/// </summary>
public class SlalRejectionsEpoch : DreamersEpoch<SlalStory>
{
    public override EpochEra Era => EpochEra.Prehistoria2;
    public override int EraPosition => 3;
    protected override string PortraitFile => "slal/rejections";

    public override IReadOnlyList<PotionModel> Potions =>
    [
        ModelDb.Potion<SlimePotion>(),
        ModelDb.Potion<CottonCandy>(),
        ModelDb.Potion<FlosBrew>(),
    ];
}

/// <summary>
/// 3. Defeat 15 elites with Slal. Column of "Devastation" (Ironclad 3).
/// </summary>
public class SlalMimickeryEpoch : DreamersEpoch<SlalStory>
{
    public override EpochEra Era => EpochEra.Seeds3;
    public override int EraPosition => 3;
    protected override string PortraitFile => "slal/mimickery";

    public override IReadOnlyList<CardModel> Cards =>
    [
        ModelDb.Card<SlalAbsorption>(),
        ModelDb.Card<SlalReactiveShape>(),
        ModelDb.Card<SlalBlobbishCrouch>(),
    ];
}

/// <summary>
/// 4. Win a run in Ascension 1 with Slal. Column of "The Spire" (Event 3).
/// </summary>
public class SlalCreativeSoulEpoch : DreamersEpoch<SlalStory>
{
    public override EpochEra Era => EpochEra.Blight1;
    public override int EraPosition => 4;
    protected override string PortraitFile => "slal/a_creative_soul";

    public override IReadOnlyList<CardModel> Cards =>
    [
        ModelDb.Card<SlalCreativeSpirit>(),
        ModelDb.Card<SlalShareSecrets>(),
        ModelDb.Card<SlalDraftPhase>(),
    ];
}

/// <summary>
/// 5. Complete Act 2 with Slal. Column of "Ablaze" (Ironclad 5).
/// </summary>
public class SlalInfiniteWorldsEpoch : DreamersEpoch<SlalStory>
{
    public override EpochEra Era => EpochEra.Blight2;
    public override int EraPosition => 2; // Free gap between Defect 6 and Ablaze
    protected override string PortraitFile => "slal/infinite_worlds";

    public override IReadOnlyList<RelicModel> Relics =>
    [
        ModelDb.Relic<Juicer>(),
        ModelDb.Relic<AppleSauce>(),
        ModelDb.Relic<WhiteRibbon>(),
    ];
}

/// <summary>
/// 6. Defeat 15 bosses with Slal. Column of "The Calm" (Potion 2).
/// </summary>
public class SlalSelfDiscoveryEpoch : DreamersEpoch<SlalStory>
{
    public override EpochEra Era => EpochEra.Flourish0;
    public override int EraPosition => 3;
    protected override string PortraitFile => "slal/self_discovery";

    public override IReadOnlyList<RelicModel> Relics =>
    [
        ModelDb.Relic<GeniesAdvice>(),
        ModelDb.Relic<Champagne>(),
        ModelDb.Relic<EvilWaffle>(),
    ];
}

/// <summary>
/// 7. Play a run with Shion. Unlocks Slal. Column of "Futility" (Necrobinder 3).
/// </summary>
public class SlalReconnaissanceEpoch : CharacterUnlockEpoch<SlalTheEndless, SlalStory>
{
    public override EpochEra Era => EpochEra.Invitation1;
    public override int EraPosition => 6; // Above Shion's The Call
    protected override string PortraitFile => "slal/reconnaissance";
}

/// <summary>
/// 8. Unlock the Cyan Chaos Emerald (win a run with it). Column of Shion's "Taste of Power".
/// </summary>
public class SlalMistakeEpoch : DreamersEpoch<SlalStory>
{
    public override EpochEra Era => EpochEra.Invitation6;
    public override int EraPosition => 5;
    protected override string PortraitFile => "slal/mistake";

    public override IReadOnlyList<CardModel> Cards =>
    [
        ModelDb.Card<SlalRunningJoke>(),
        ModelDb.Card<SlalDiceShotgun>(),
        ModelDb.Card<SlalContinuityThread>(),
    ];

    // Same as ChaosEmeraldUnlockPatch: winning with the Emerald (solo or combo) unlocks it
    public override bool IsEarnedPostRun(SerializablePlayer player, SerializableRun run, bool victory) =>
        victory
        && player.Relics.Any(r => r.Id == ModelDb.Relic<CyanChaosEmerald>().Id || r.Id == ModelDb.Relic<CyanChaosEmeralds>().Id);
}
