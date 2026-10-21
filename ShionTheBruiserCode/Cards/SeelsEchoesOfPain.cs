using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Shuffle a copy of ALL your enchanted Wounds into your Draw Pile. Draw cards until your Hand is full. Exhaust.
/// </summary>
public class SeelsEchoesOfPain() : SeelsTheRecklessCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Wound>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var wounds = SeelsMechanicsHelper.GetAllCombatCards(Owner)
            .Where(c => SeelsMechanicsHelper.IsWound(c) && c.Enchantment is not null)
            .ToList();

        foreach (var wound in wounds)
        {
            var copy = wound.CreateClone();
            // The copies act as if they had never been played (Sown, Swift...).
            if (copy.Enchantment is not null) copy.Enchantment.Status = EnchantmentStatus.Normal;
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Draw, Owner, CardPilePosition.Random);
        }

        var toDraw = CardPile.MaxCardsInHand - PileType.Hand.GetPile(Owner).Cards.Count;
        if (toDraw > 0)
        {
            await CardPileCmd.Draw(choiceContext, toDraw, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1); //todo : upgrade not specified
    }
}
