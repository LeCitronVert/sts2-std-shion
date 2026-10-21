using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Request 1. Draw cards until your Hand is full.
/// </summary>
public class FryBottomlessMagazines() : FryTheCommanderCard(1, //todo : choose cost
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FryCardKeywords.Request];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Request", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FryMechanicsHelper.Request(choiceContext, this, DynamicVars["Request"].IntValue, async ctx =>
        {
            var toDraw = CardPile.MaxCardsInHand - PileType.Hand.GetPile(Owner).Cards.Count;
            if (toDraw > 0)
            {
                await CardPileCmd.Draw(ctx, toDraw, Owner);
            }
        });
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1); //todo : upgrade not specified
    }
}
