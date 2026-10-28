using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Relics;
using ShionTheBruiser.ShionTheBruiserCode.Timeline;
using ShionTheBruiser.ShionTheBruiserCode.Timeline.BaseLibEpochs;

namespace ShionTheBruiser.ShionTheBruiserCode.Character;

public class FryTheCommander : PlaceholderCharacterModel, IEpochCharacter
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

    // Timeline (Timeline/FryEpochs.cs)
    protected override CharacterModel UnlocksAfterRunAs => ModelDb.Character<SeelsTheReckless>();
    public EpochModel? UnlockEpoch => EpochModel.Get<FryUnlockEpoch>();

    // Couleurs
    public override Color MapDrawingColor => Color;

    public override Color RemoteTargetingLineColor => Color;

    public override Color RemoteTargetingLineOutline => Color;
    public override Color DialogueColor => Color;
}
