using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Always Novel. Add a copy of ALL cards in your Hand into your Draw Pile.
/// Gain Block equal to double the number of cards created this way. Exhaust (upgrade: no Exhaust).
/// </summary>
public class SlalWallOfText(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(2),
        // This card isn't copied (it's being played)
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((model, _) =>
            PileType.Hand.GetPile(model.Owner).Cards.Count(c => c != model)
        ),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);

        var copies = PileType.Hand.GetPile(card.Owner).Cards.Where(c => c != card).Select(c => c.CreateClone()).ToList();
        if (copies.Count == 0) return;

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Draw, card.Owner, CardPilePosition.Random));

        var block = copies.Count * card.DynamicVars.CalculationExtra.BaseValue;
        await CreatureCmd.GainBlock(card.Owner.Creature, block, card.DynamicVars.CalculatedBlock.Props, play);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

    public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<SlalAnvilGag>();
}
