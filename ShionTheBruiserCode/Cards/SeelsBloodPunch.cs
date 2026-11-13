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
/// Lose 3 (2) HP. Maim 1. Deal 7 damage. Increase this card's damage by 3 for each time you lost HP this combat.
/// Put this card into your Draw Pile.
/// The HP lost by this play counts (it happens before the damage).
/// </summary>
public class SeelsBloodPunch() : SeelsTheRecklessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SeelsCardKeywords.Maim];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        MaimHelper.Var(1M),
        new HpLossVar(3M),
        new CalculationBaseVar(7M),
        new ExtraDamageVar(3M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => PreviewTimesLostHp(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Blood>()];

    /// <summary>Times lost HP this combat, counting the HP this card is about to make you lose.</summary>
    private static int PreviewTimesLostHp(CardModel card)
    {
        if (card.CombatState is null) return 0;
        return SeelsMechanicsHelper.TimesLostHpThisCombat(card.Owner.Creature) + (card.DynamicVars.HpLoss.IntValue > 0 ? 1 : 0);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await SeelsMechanicsHelper.LoseHp(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, this);
        // The HP just lost is already in the history here, so count it directly (not through the preview).
        var damage = DynamicVars.CalculationBase.BaseValue
            + DynamicVars.ExtraDamage.BaseValue * SeelsMechanicsHelper.TimesLostHpThisCombat(Owner.Creature);
        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);
    }

#if STS2_BETA
    public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation location)
    {
        var (pileType, position) = ModifyResultPile(card, location.pileType, location.position);
        return location with { pileType = pileType, position = position };
    }
#else
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position) =>
        ModifyResultPile(card, pileType, position);
#endif

    private (PileType, CardPilePosition) ModifyResultPile(CardModel card, PileType pileType, CardPilePosition position)
    {
        return card == this && pileType == PileType.Discard ? (PileType.Draw, CardPilePosition.Random) : (pileType, position);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.HpLoss.UpgradeValueBy(-1M);
    }
}
