using System.Runtime.InteropServices;
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

public class ShionTheBruiser : PlaceholderCharacterModel
{
    public const string CharacterId = "ShionTheBruiser";

    public static readonly Color Color = new("9234eb");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<ShionStrike>(),
        ModelDb.Card<ShionStrike>(),
        ModelDb.Card<ShionStrike>(),
        ModelDb.Card<ShionStrike>(),
        ModelDb.Card<ShionDefend>(),
        ModelDb.Card<ShionDefend>(),
        ModelDb.Card<ShionDefend>(),
        ModelDb.Card<ShionDefend>(),
        ModelDb.Card<ShionFlood>(),
        ModelDb.Card<ShionBluff>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BattleRags>(),
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<ShionTheBruiserCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<ShionTheBruiserRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<ShionTheBruiserPotionPool>();
    
    // Exclusive animations
    public static string GetEnergyAnimIfApplicable(CharacterModel character)
    {
        return character is not ShionTheBruiser ? "Attack" : "Energy";
    }
    
    public static float GetEnergyDelayIfApplicable(CharacterModel character)
    {
        return character is not ShionTheBruiser ? character.AttackAnimDelay : 0.2f;
    }
    
    public static string GetEnergyCastAnimIfApplicable(CharacterModel character)
    {
        return character is not ShionTheBruiser ? "Cast" : "Energy";
    }
    
    public static float GetEnergyCastDelayIfApplicable(CharacterModel character)
    {
        return character is not ShionTheBruiser ? character.CastAnimDelay : 0.2f;
    }

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "shion/ui_icon_shion.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "shion/ui_portrait_shion.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "shion/ui_map_taco.png".CharacterUiPath();
    
    public override string? CustomIconOutlineTexturePath => "shion/ui_icon_shion_white.png".CharacterUiPath();

    public override List<string> GetArchitectAttackVfx()
    {
        int num = 20;
        List<string> list = new List<string>(num);
        CollectionsMarshal.SetCount<string>(list, num);
        Span<string> span = CollectionsMarshal.AsSpan<string>(list);
        for (int i = 0; i < 20; i++)
        {
            span[i] = "vfx/vfx_heavy_blunt";
        }
        return list;
    }
    
    public override NCreatureVisuals CreateCustomVisuals()
    {
        return NodeFactory<NCreatureVisuals>.CreateFromScene(Path.Join(MainFile.ResPath, "scenes",
            "shion_character.tscn"));
    }

    public override string CustomMerchantAnimPath => Path.Join(MainFile.ResPath, "scenes",
        "shion_merchant.tscn");
    
    public override string CustomRestSiteAnimPath => Path.Join(MainFile.ResPath, "scenes",
        "shion_rest.tscn");
    

    public override RelicIconData CustomYummyCookie => new(
        "shion.png".CookieImagePath(),
        "shion.png".CookieImagePath(),
        "shion_outline.png".CookieImagePath()
    );
    
    public override string CustomEnergyCounterPath => Path.Join(MainFile.ResPath, "scenes", "shion_energy.tscn");
    
    // Couleurs
    public override Color MapDrawingColor => Color;

    public override Color RemoteTargetingLineColor => Color;

    public override Color RemoteTargetingLineOutline => Color;
    public override Color DialogueColor => Color;
}