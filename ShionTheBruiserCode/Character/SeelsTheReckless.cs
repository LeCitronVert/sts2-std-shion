using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class SeelsTheReckless : PlaceholderCharacterModel
{
    public const string CharacterId = "SeelsTheReckless";

    public static readonly Color Color = new("add8e6");

    // No custom visuals yet: borrow the Silent's placeholder assets.
    public override string PlaceholderID => "silent";

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
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<ThornBracelet>(),
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<SeelsTheRecklessCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SeelsTheRecklessRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SeelsTheRecklessPotionPool>();

    // Couleurs
    public override Color MapDrawingColor => Color;

    public override Color RemoteTargetingLineColor => Color;

    public override Color RemoteTargetingLineOutline => Color;
    public override Color DialogueColor => Color;
}
