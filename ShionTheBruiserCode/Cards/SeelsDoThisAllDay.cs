using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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
/// Put a copy of ALL Slashes from your Exhaust Pile into your Draw Pile (and upgrade them).
/// The originals stay in the Exhaust Pile.
/// </summary>
public class SeelsDoThisAllDay() : SeelsTheRecklessCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("SlashesAmount").WithMultiplier((card, _) => GetExhaustedSlashes(card).Count),
    ];

    private static List<CardModel> GetExhaustedSlashes(CardModel card) =>
        PileType.Exhaust.GetPile(card.Owner).Cards.Where(SeelsMechanicsHelper.IsSlash).ToList();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SeelsSlash>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var slashes = GetExhaustedSlashes(this);
        if (slashes.Count == 0) return;

        var copies = slashes.Select(s => s.CreateClone()).ToList();
        if (IsUpgraded)
        {
            foreach (var copy in copies.Where(c => c.IsUpgradable))
            {
                CardCmd.Upgrade(copy);
            }
        }
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Draw, Owner, CardPilePosition.Random));
    }

    protected override void OnUpgrade()
    {
    }
}
