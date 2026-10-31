using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Memory. Deal 14 (20) damage. Apply Weak equal to your Random.
/// Increase this card's damage by 4 (5) each time you gain Flashback this combat.
/// </summary>
public class SlalAnvilGag() : SlalTheEndlessCard(1,
    CardType.Attack, CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Memory>(1),
        new CalculationBaseVar(14),
        new ExtraDamageVar(4),
        // In the Hand, also counts the Flashback its own Memory gives before the hit
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            SlalMechanicsHelper.GetTimesFlashbackGained(card.Owner.Creature) + (card.Pile?.Type == PileType.Hand ? 1 : 0)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Memory>(),
        HoverTipFactory.FromPower<Flashback>(),
        HoverTipFactory.FromPower<Powers.Random>(),
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        // Its Flashback raises the damage before the hit
        await SlalMechanicsHelper.GainMemory(choiceContext, card.Owner.Creature, card.DynamicVars["Memory"].BaseValue, card);

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
        DynamicVars.CalculationBase.UpgradeValueBy(6);
        DynamicVars.ExtraDamage.UpgradeValueBy(1);
    }
}
