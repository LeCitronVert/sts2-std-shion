using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionNightSlash() : CritContributorCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy), ICritAttackRateContributor
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(25, ValueProp.Move),
        new CritAttackRateVar(50),
        new CritDisplayTooltipVar()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars[CritAttackRateVar.Key].UpgradeValueBy(50);
    }

    public decimal GetCritAttackRateBonus(CardModel? sourceCard)
    {
        return sourceCard == this ? DynamicVars[CritAttackRateVar.Key].BaseValue / 100 : 0;
    }
}