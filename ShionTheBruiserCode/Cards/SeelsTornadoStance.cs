using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
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
/// Retain. Add Replay to ALL your Slashes. Gain 1 Vulnerable for each of your Slashes (not the Exhausted ones).
/// At the start of your turn, if this is in your Hand, add 1 (2) Slash(es) into your Discard Pile.
/// </summary>
public class SeelsTornadoStance() : SeelsTheRecklessCard(2,
    CardType.Skill, CardRarity.Ancient,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("VulnerableAmount").WithMultiplier((card, _) => GetSlashes(card).Count),
    ];

    private static List<CardModel> GetSlashes(CardModel card) =>
        SeelsMechanicsHelper.GetAllCombatCards(card.Owner)
            .Where(c => SeelsMechanicsHelper.IsSlash(c) && c.Pile?.Type != PileType.Exhaust)
            .ToList();

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SeelsSlash>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Pile?.Type != PileType.Hand) return;
        await SeelsMechanicsHelper.AddSlashes(Owner, DynamicVars.Cards.IntValue, PileType.Discard, false);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var slashes = GetSlashes(this);
        foreach (var slash in slashes)
        {
            slash.BaseReplayCount += 1;
            SeelsMechanicsHelper.CountModification(slash);
        }

        if (slashes.Count > 0)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, slashes.Count, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1M);
    }
}
