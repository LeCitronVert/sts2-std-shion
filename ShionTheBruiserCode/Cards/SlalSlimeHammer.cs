using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Very Slimy. Deal 6 damage. Deals 4 (6) additional damage for each Slimy card you have.
/// </summary>
public class SlalSlimeHammer() : SlalSlimyCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.VerySlimy];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(6),
        new ExtraDamageVar(4),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            card.Owner.PlayerCombatState?.AllCards.Count(c => SlalMechanicsHelper.IsSlimy(c)) ?? 0),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.CalculatedDamage)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(2);
    }
}
