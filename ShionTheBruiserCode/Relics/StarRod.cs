using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Shion's Tanx relic (ShionAncientRelicPatches). Galvanised Attacks (Attacks whose own Galvanised effect triggered)
/// apply, at random, 1 Weak, 1 Vulnerable, 6 Confusion, 6 Doom or 3 Poison to the enemies they hit.
/// One roll per play, applied once the Attack resolves.
/// </summary>
[Pool(typeof(EventRelicPool))]
public class StarRod : ShionTheBruiserRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1),
        new PowerVar<VulnerablePower>(1),
        new PowerVar<Confusion>(6),
        new PowerVar<DoomPower>(6),
        new PowerVar<PoisonPower>(3),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromPower<PoisonPower>(),
    ];

    // Enemies hit by the Attack currently being played (random-target Attacks only know them once they hit)
    private CardModel? _currentAttack;
    private List<Creature> _hitEnemies = [];

    // MemberwiseClone shares the list with the canonical model, so give each instance its own.
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _hitEnemies = [];
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner != Owner || card.Type != CardType.Attack) return Task.CompletedTask;

        // Each play (replays included) starts fresh
        ShionMechanicsHelper.ClearCardGalvanised(card);
        _currentAttack = card;
        _hitEnemies.Clear();
        return Task.CompletedTask;
    }

    public override Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (cardSource == null || cardSource != _currentAttack || target.IsPlayer || _hitEnemies.Contains(target))
            return Task.CompletedTask;

        _hitEnemies.Add(target);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != _currentAttack) return;

        var targets = _hitEnemies.Where(e => e.IsAlive).ToList();
        _currentAttack = null;
        _hitEnemies.Clear();

        if (!ShionMechanicsHelper.WasCardGalvanised(card) || targets.Count == 0) return;

        Flash();
        var owner = Owner.Creature;
        // Powers and relics apply Confusion with no card source, so Random doesn't boost it
        switch (Owner.RunState.Rng.Niche.NextInt(5))
        {
            case 0:
                await PowerCmd.Apply<WeakPower>(choiceContext, targets, DynamicVars["WeakPower"].BaseValue, owner, null);
                break;
            case 1:
                await PowerCmd.Apply<VulnerablePower>(choiceContext, targets, DynamicVars["VulnerablePower"].BaseValue, owner, null);
                break;
            case 2:
                await PowerCmd.Apply<Confusion>(choiceContext, targets, DynamicVars["Confusion"].BaseValue, owner, null);
                break;
            case 3:
                await PowerCmd.Apply<DoomPower>(choiceContext, targets, DynamicVars["DoomPower"].BaseValue, owner, null);
                break;
            default:
                await PowerCmd.Apply<PoisonPower>(choiceContext, targets, DynamicVars["PoisonPower"].BaseValue, owner, null);
                break;
        }
    }
}
