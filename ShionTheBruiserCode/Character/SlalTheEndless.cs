using BaseLib.Abstracts;
using BaseLib.Patches.UI;
using BaseLib.Utils.NodeFactories;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;


public class SlalTheEndless : PlaceholderCharacterModel
{
    public const string CharacterId = "SlalTheEndless";

    public static readonly Color Color = new("f1c232");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 75;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<SlalStrike>(),
        ModelDb.Card<SlalStrike>(),
        ModelDb.Card<SlalStrike>(),
        ModelDb.Card<SlalStrike>(),
        ModelDb.Card<SlalDefend>(),
        ModelDb.Card<SlalDefend>(),
        ModelDb.Card<SlalDefend>(),
        ModelDb.Card<SlalDefend>(),
        ModelDb.Card<SlalWallOfText>(),
        ModelDb.Card<SlalSeeWhatSticks>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<CreativeSpark>(),
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<SlalTheEndlessCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SlalTheEndlessRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SlalTheEndlessPotionPool>();
    
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

    public override string CustomIconTexturePath => "slal/ui_icon_slal.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "slal/ui_portrait_slal.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "slal/UI_map_slal.png".CharacterUiPath();
    
    public override string? CustomIconOutlineTexturePath => "slal/ui_icon_slal_white.png".CharacterUiPath();
    
    public override NCreatureVisuals CreateCustomVisuals()
    {
        return NodeFactory<NCreatureVisuals>.CreateFromScene(Path.Join(MainFile.ResPath, "scenes",
            "slal_character.tscn"));
    }

    public override string CustomMerchantAnimPath => Path.Join(MainFile.ResPath, "scenes",
        "slal_merchant.tscn");

    public override RelicIconData CustomYummyCookie => new(
        "slal.png".CookieImagePath(),
        "slal.png".CookieImagePath(),
        "slal_outline.png".CookieImagePath()
    );
    
    // Couleurs
    
    public override Color MapDrawingColor => Color;

    public override Color RemoteTargetingLineColor => Color;

    public override Color RemoteTargetingLineOutline => Color;
    public override Color DialogueColor => Color;
}

