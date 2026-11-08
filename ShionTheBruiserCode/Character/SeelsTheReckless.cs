using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class SeelsTheReckless : PlaceholderCharacterModel, IEpochCharacter, ICharacterSelectSkin
{
    public const string CharacterId = "SeelsTheReckless";

    public static readonly Color Color = new("f1c232");

    // No custom visuals yet: borrow the Silent's placeholder assets.
    public override string PlaceholderID => "silent";

    // Character select → run wipe. Shader driven by NTransition's threshold tween.
    public override string CustomCharacterSelectTransitionPath =>
        $"{MainFile.ResPath}/materials/transitions/seels_transition_mat.tres";

    // The Silent's own wipe, the only other one vanilla has; fits the blade work.
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_silent";

    // Character select playstyle icons (Patches/CharacterSelectSkinPatch.cs), text in characters.json
    public IReadOnlyList<CharacterPlaystyle> Playstyles { get; } =
    [
        CharacterPlaystyle.Power<Blood>("BLOOD_BEND"),
        CharacterPlaystyle.Power<GamblersLuck>("GAMBLE"),
        CharacterPlaystyle.Power<ComboMeter>("SLASH"),
        CharacterPlaystyle.Power<BleedInstructions>("WOUNDS"),
        CharacterPlaystyle.Power<Gloat>("GLOAT"),
    ];

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 75;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<SeelsStrike>(),
        ModelDb.Card<SeelsStrike>(),
        ModelDb.Card<SeelsStrike>(),
        ModelDb.Card<SeelsStrike>(),
        ModelDb.Card<SeelsDefend>(),
        ModelDb.Card<SeelsDefend>(),
        ModelDb.Card<SeelsDefend>(),
        ModelDb.Card<SeelsDefend>(),
        ModelDb.Card<SeelsBloodCage>(),
        ModelDb.Card<SeelsBloodDrop>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<ThornBracelet>(),
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<SeelsTheRecklessCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SeelsTheRecklessRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SeelsTheRecklessPotionPool>();

    // Timeline (Timeline/SeelsEpochs.cs). Condensed Might has its own condition.
    protected override CharacterModel UnlocksAfterRunAs => ModelDb.Character<SlalTheEndless>();
    public EpochModel? UnlockEpoch => EpochModel.Get<SeelsUnlockEpoch>();
    public EpochModel? Act1Epoch => EpochModel.Get<SeelsBoringEpoch>();
    public EpochModel? FifteenElitesEpoch => EpochModel.Get<SeelsHoldOnEpoch>();
    public EpochModel? Act2Epoch => EpochModel.Get<SeelsWaitForMeGuysEpoch>();
    public EpochModel? Act3Epoch => EpochModel.Get<SeelsWellThisBlowsEpoch>();
    public EpochModel? FifteenBossesEpoch => EpochModel.Get<SeelsRagtagPalsEpoch>();
    public EpochModel? AscensionOneEpoch => EpochModel.Get<SeelsRollWithThePunchesEpoch>();

    // Couleurs
    public override Color MapDrawingColor => Color;

    public override Color RemoteTargetingLineColor => Color;

    public override Color RemoteTargetingLineOutline => Color;
    public override Color DialogueColor => Color;
}
