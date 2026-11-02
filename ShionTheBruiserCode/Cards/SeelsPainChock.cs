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
/// Wounding. Deal 7 (9) damage to ALL enemies. Deal 3 (4) additional damage per Wound in your Hand.
/// </summary>
public class SeelsPainChock() : SeelsTheRecklessCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SeelsCardKeywords.Wounding];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(7M),
        new ExtraDamageVar(3M),
        // Counts the Wound this card adds before attacking
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => SeelsMechanicsHelper.PreviewWoundsInHandAfterWounding(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Wound>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;
        await SeelsMechanicsHelper.AddWounds(Owner, 1);

        var wounds = SeelsMechanicsHelper.GetWoundsInHand(Owner).Count();
        var damage = DynamicVars.CalculationBase.BaseValue + DynamicVars.ExtraDamage.BaseValue * wounds;
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2M);
        DynamicVars.ExtraDamage.UpgradeValueBy(1M);
    }
}
