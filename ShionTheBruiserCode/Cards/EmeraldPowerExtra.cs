using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Strife;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Grey Chaos Emerald Burst (Emerald Power, 3 (2)-cost Power): draw cards until your hand is full. Your hand is free to
/// play this turn (the cards in it once the drawing is done).
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerExtra() : EmeraldPowerCard(3, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Grey;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("DrawAmount").WithMultiplier((card, _) => ((EmeraldPowerExtra)card).PreviewDraw()),
    ];

    private static int SpaceInHand(CardModel card, IEnumerable<CardModel> hand) => Math.Max(0, CardPile.MaxCardsInHand - hand.Count());

    // This card has left the Hand when it resolves
    private decimal PreviewDraw() => CombatState is null ? 0 : SpaceInHand(this, PreviewHelper.OtherCardsInHand(this));

    protected override async Task OnEmeraldPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var space = SpaceInHand(this, PileType.Hand.GetPile(Owner).Cards);
        if (space > 0) await CardPileCmd.Draw(choiceContext, space, Owner);
        foreach (var card in PileType.Hand.GetPile(Owner).Cards.ToList()) card.SetToFreeThisTurn();
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
