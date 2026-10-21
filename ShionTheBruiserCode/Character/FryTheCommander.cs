using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class FryTheCommander : PlaceholderCharacterModel
{
    public const string CharacterId = "FryTheCommander";

    public static readonly Color Color = new("cc0000");

    // No custom visuals yet: borrow the Regent's placeholder assets (he also uses Stars).
    public override string PlaceholderID => "regent";

    public override bool ShouldAlwaysShowStarCounter => true;

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 80;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<FryStrike>(),
        ModelDb.Card<FryStrike>(),
        ModelDb.Card<FryStrike>(),
        ModelDb.Card<FryStrike>(),
        ModelDb.Card<FryDefend>(),
        ModelDb.Card<FryDefend>(),
        ModelDb.Card<FryDefend>(),
        ModelDb.Card<FryDefend>(),
        ModelDb.Card<FryRefugeInAudacity>(),
        ModelDb.Card<FryRuinWalk>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<FryRelicTodo1>(),
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<FryTheCommanderCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<FryTheCommanderRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<FryTheCommanderPotionPool>();

    // Couleurs
    public override Color MapDrawingColor => Color;

    public override Color RemoteTargetingLineColor => Color;

    public override Color RemoteTargetingLineOutline => Color;
    public override Color DialogueColor => Color;
}
