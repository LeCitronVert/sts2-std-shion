using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionOffRockers() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3),
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var player = card.Owner;

        // Grab the Colorless card pool options (same pattern as ShionImprovise)
        var colorlessOptions = ModelDb.CardPool<ColorlessCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).ToList();

        List<CardModel> selection = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Draw.GetPile(card.Owner),
            card.Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, DynamicVars.Cards.IntValue)
        )).ToList<CardModel>();

        foreach (CardModel original in selection)
        {
            await CardCmd.Transform(
                new CardTransformation(original, colorlessOptions).Yield(),
                card.Owner.RunState.Rng.CombatCardSelection
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2);
    }
}