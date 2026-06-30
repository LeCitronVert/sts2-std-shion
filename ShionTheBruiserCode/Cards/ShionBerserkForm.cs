using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionBerserkForm() : ShionTheBruiserCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<BerserkForm>(30),
        new DynamicVar("StrengthLoss", BerserkForm.StrengthLoss)
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<BerserkForm>(choiceContext, card.Owner.Creature, DynamicVars["BerserkForm"].BaseValue, card.Owner.Creature, card);
        
        
        var randomOptions = ShionMechanicsHelper.GetRandomCardOptionsForPool([ModelDb.CardPool<ShionTheBruiserCardPool>(), ModelDb.CardPool<ColorlessCardPool>()
        ], card.Owner).Where(c => c.Type == CardType.Attack).ToList();
        
        // Transforms all cards that are not Attacks into random Shion and Colorless Attacks
        foreach (CardModel original in ShionMechanicsHelper.GetAllCombatCards(card.Owner).Where(c => c.IsTransformable && c.Type != CardType.Attack).ToList())
        {
            await CardCmd.Transform(
                new CardTransformation(original, randomOptions).Yield(),
                card.Owner.RunState.Rng.CombatCardSelection
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BerserkForm"].BaseValue += 20;
    }
}