using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalAvatarCharacter() : SlalTheEndlessCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ConfusionAmount").WithMultiplier((card, _) => GetPowersPlayed(card).Count),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];

    private static List<CardModel> GetPowersPlayed(CardModel card)
    {
        return CombatManager.Instance.History.CardPlaysFinished
            .Where(e => e.CardPlay.Card.Owner == card.Owner && e.CardPlay.IsFirstInSeries && e.CardPlay.Card.Type == CardType.Power)
            .Select(e => e.CardPlay.Card)
            .ToList();
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        var powers = GetPowersPlayed(card);
        if (powers.Count == 0) return;

        await PowerCmd.Apply<Confusion>(choiceContext, play.Target, powers.Count, card.Owner.Creature, card);

        var copies = powers.Select(p => p.CreateClone()).ToList();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Discard, card.Owner));
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
