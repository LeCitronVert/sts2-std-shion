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
/// Request 2 (1). Choose 2 cards in your Draw Pile to put into your Hand.
/// </summary>
public class FryGratuitousEnglish() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FryCardKeywords.Request];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Request", 2M),
        new CardsVar(2),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FryMechanicsHelper.Request(choiceContext, this, DynamicVars["Request"].IntValue, async ctx =>
        {
            var cards = await CardSelectCmd.FromCombatPile(
                ctx,
                PileType.Draw.GetPile(Owner),
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, DynamicVars.Cards.IntValue)
            );
            await CardPileCmd.Add(cards, PileType.Hand);
        });
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Request"].UpgradeValueBy(-1M);
    }
}
