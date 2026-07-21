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

    public static readonly Color Color = new("4a90e2");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 60;

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

    public override CardPoolModel CardPool => ModelDb.CardPool<SlalTheEndlessCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SlalTheEndlessRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SlalTheEndlessPotionPool>();
}

