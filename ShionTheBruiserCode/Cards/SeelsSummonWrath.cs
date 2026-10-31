using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Extensions;
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
/// Lose X HP (1 HP). Put X random Slashes from your Draw or Discard Piles into your Hand and Upgrade them.
/// </summary>
public class SeelsSummonWrath() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("XAmount").WithMultiplier((card, _) => PreviewHelper.XValue(card)),
        new CalculatedVar("SlashesAmount").WithMultiplier((card, _) =>
            Math.Min(PreviewHelper.XValue(card), GetSlashesInPiles(card).Count)),
    ];

    private static List<CardModel> GetSlashesInPiles(CardModel card) =>
        PileType.Draw.GetPile(card.Owner).Cards
            .Concat(PileType.Discard.GetPile(card.Owner).Cards)
            .Where(SeelsMechanicsHelper.IsSlash)
            .ToList();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SeelsSlash>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var x = ResolveEnergyXValue();
        await SeelsMechanicsHelper.LoseHp(choiceContext, Owner.Creature, IsUpgraded ? 1 : x, this);

        var slashes = GetSlashesInPiles(this)
            .StableShuffle(Owner.RunState.Rng.CombatCardSelection)
            .Take(x)
            .ToList();

        foreach (var slash in slashes)
        {
            if (slash.IsUpgradable) CardCmd.Upgrade(slash);
            await CardPileCmd.Add(slash, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
