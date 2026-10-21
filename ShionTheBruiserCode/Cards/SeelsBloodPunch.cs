using MegaCrit.Sts2.Core.Commands;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Lose 1 HP. Deal 7 damage. This combat, increase this card's damage by 3 and HP loss by 1.
/// Shuffle this card into your Draw Pile (put it on top of your Draw Pile instead).
/// </summary>
public class SeelsBloodPunch() : SeelsTheRecklessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7M, ValueProp.Move),
        new HpLossVar(1M),
        new DynamicVar("DamageIncrease", 3M),
        new DynamicVar("HpLossIncrease", 1M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await SeelsMechanicsHelper.LoseHp(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);

        DynamicVars.Damage.BaseValue += DynamicVars["DamageIncrease"].BaseValue;
        DynamicVars.HpLoss.BaseValue += DynamicVars["HpLossIncrease"].BaseValue;
    }

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        return card == this && pileType == PileType.Discard
            ? (PileType.Draw, IsUpgraded ? CardPilePosition.Top : CardPilePosition.Random)
            : (pileType, position);
    }

    protected override void OnUpgrade()
    {
    }
}
