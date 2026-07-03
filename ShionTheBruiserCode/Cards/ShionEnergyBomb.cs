using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Critical;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyBomb() : CritContributorCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies), ICritHitRateContributor
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(30, ValueProp.Move),
        new CritAttackRateVar(0)
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState != null)
            await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue).FromCard(card)
                .TargetingAllOpponents(card.CombatState).WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
                .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CritAttackRate"].UpgradeValueBy(25);
    }

    public decimal GetCritHitRateBonus(CardModel? sourceCard) =>
        sourceCard == this ? sourceCard.DynamicVars[CritAttackRateVar.Key].BaseValue / 100 : 0m;
}