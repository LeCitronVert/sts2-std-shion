using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 8 damage. Apply Weak equal to your Random.
/// Increase this card's damage by 2 each time you gain Random this combat. (Retain)
/// </summary>
public class SlalAnvilGag() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(8),
        new ExtraDamageVar(2),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => SlalMechanicsHelper.GetTimesRandomGained(card.Owner.Creature)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Powers.Random>(),
        HoverTipFactory.FromPower<WeakPower>(),
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

        var random = SlalMechanicsHelper.GetRandom(card.Owner.Creature);
        if (random > 0 && play.Target.IsAlive)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, random, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
