using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class SeelsTheReckless : PlaceholderCharacterModel, IEpochCharacter, ICharacterSelectSkin
{
    public const string CharacterId = "SeelsTheReckless";

    public static readonly Color Color = new("f1c232");

    // Merchant, rest site and energy counter have no custom art yet: they still borrow the Silent's.
    public override string PlaceholderID => "silent";

    // Custom visuals
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "seels/ui_icon_seels.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "seels/ui_portrait_seels.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "seels/UI_map_seels.png".CharacterUiPath();

    public override string? CustomIconOutlineTexturePath => "seels/ui_icon_seels_white.png".CharacterUiPath();

    // Claws: 1 weak, 1 strong, 3 weak (the Architect shuffles the order)
    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_attack_slash",
        "vfx/vfx_attack_slash",
        "vfx/vfx_attack_slash",
    ];

    // Animations: idle, attack, cast, blood (Blood Bend), hit. Multi-hit attacks restart on the slash frame.
    // Cast: Singletons/SeelsAnimationSingleton.cs, blood: SeelsMechanicsHelper.BloodBend.
    public override NCreatureVisuals CreateCustomVisuals()
    {
        return NodeFactory<NCreatureVisuals>.CreateFromScene(Path.Join(MainFile.ResPath, "scenes",
            "seels_character.tscn"));
    }

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
#if STS2_BETA
    // Dread banner name tint; vanilla uses its class colour at C0 (~0.75) alpha.
    public override Color DreadNameColor => new(Color, 0.75f);
#endif
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
