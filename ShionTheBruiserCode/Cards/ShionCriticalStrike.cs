using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionCriticalStrike() : ShionTheBruiserCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy), ICritHitRateContributor
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(14M, ValueProp.Move), 
        new DamageCapVar(30M),
        new CritDisplayTooltipVar(),
        new CritAttackRateVar(100)
    ];
    
    public decimal GetCritHitRateBonus(CardModel? sourceCard) =>
        sourceCard == this ? sourceCard.DynamicVars[CritAttackRateVar.Key].BaseValue : 0m;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionCriticalStrike card = this;
        ArgumentNullException.ThrowIfNull(play.Target, "play.Target");
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars[DamageCapVar.Key].UpgradeValueBy(20M);
    }
}