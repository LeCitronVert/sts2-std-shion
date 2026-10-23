using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Lose all Frail. Transform ALL Slimed in your Hand into random non-Rare cards.
/// </summary>
public class SlalReshape(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<FrailPower>(),
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromCard<Slimed>(),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        await PowerCmd.Remove<FrailPower>(card.Owner.Creature);
        var slimedCards = PileType.Hand.GetPile(card.Owner).Cards.Where(c => c is Slimed && c.IsTransformable).ToList();
        if (slimedCards.Count == 0) return;

        var cardOptions = SlalMechanicsHelper.GetRandomCardOptions(card.Owner)
            .Where(c => c.CanBeGeneratedInCombat && c.Rarity is CardRarity.Common or CardRarity.Uncommon)
            .ToList();
        await CardCmd.Transform(
            slimedCards.Select(c => new CardTransformation(c, cardOptions)),
            card.Owner.RunState.Rng.CombatCardSelection
        );
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}