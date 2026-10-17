using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEleventhHour() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Bonus", EleventhHour.StrengthBonus),
        new DynamicVar("Malus", EleventhHour.StatsMalus),
        new CriticalDrawVar(5),
        new CritDisplayTooltipVar()
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<DebilitatePower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await ShionMechanicsHelper.EnsureCreatureHasCritTracker(card.Owner.Creature);
        CardModel? card2 = (PileType
                .Draw
                .GetPile(card.Owner)
                .Cards
                .Where(c => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList().StableShuffle(card.Owner.RunState.Rng.Shuffle).FirstOrDefault() ?? PileType.Draw.GetPile(card.Owner).Cards.Where(c => c.Type == CardType.Attack).ToList().StableShuffle(card.Owner.RunState.Rng.Shuffle).FirstOrDefault()) ?? null
            ;
        if (card2 != null)
            await CardPileCmd.Add(card2, PileType.Hand);
        
        await PowerCmd.Apply<EleventhHour>(
            choiceContext,
            Owner.Creature,
            1M,
            Owner.Creature,
            card
        );
    }
    
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
    }


    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
}