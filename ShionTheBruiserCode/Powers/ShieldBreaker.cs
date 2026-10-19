using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class ShieldBreaker : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public const int GalvanisedThreshold = 15;
    public const int VulnerableAmount = 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("VulnerableAmount", VulnerableAmount)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
    ];
    
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        return card.Owner.Creature == this.Owner && card.Type == CardType.Attack && target?.Block > 0 ? playCount + Amount : playCount;
    }

    // Random-target Attacks only know their targets once they hit
    private CardModel? _galvanisedRandomAttack;
    private readonly HashSet<Creature> _randomTargetsAffected = [];

    // Checked before the Attack resolves, since it consumes Vigor
    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner.Creature != Owner || card.Type != CardType.Attack || !ShionMechanicsHelper.IsGalvanised(Owner, GalvanisedThreshold))
            return;

        if (cardPlay.Target == null && card.TargetType == TargetType.RandomEnemy)
        {
            _galvanisedRandomAttack = card;
            _randomTargetsAffected.Clear();
            return;
        }

        List<Creature> targets = cardPlay.Target != null
            ? [cardPlay.Target]
            : card.TargetType == TargetType.AllEnemies
                ? card.CombatState?.HittableEnemies.ToList() ?? []
                : [];
        
        if (targets.Count == 0) return;
        
        Flash();
        await PowerCmd.Apply<VulnerablePower>(
            new ThrowingPlayerChoiceContext(),
            targets,
            VulnerableAmount,
            Owner,
            null
        );
    }
    
    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (cardSource == null || cardSource != _galvanisedRandomAttack || target.IsPlayer || !_randomTargetsAffected.Add(target))
            return;
        
        Flash();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, target, VulnerableAmount, Owner, null);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == _galvanisedRandomAttack)
        {
            _galvanisedRandomAttack = null;
            _randomTargetsAffected.Clear();
        }
        return Task.CompletedTask;
    }
    
    // Galvanised Power stuff
    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Owner == Owner)
        {
            ChangePulse();
        }
        return Task.CompletedTask;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ChangePulse();
        
        return Task.CompletedTask;
    }
    

    public void ChangePulse()
    {
        var shouldPulse = ShionMechanicsHelper.IsGalvanised(Owner, GalvanisedThreshold);

        if (shouldPulse)
        {
            StartPulsing();
        }
        else
        {
            StopPulsing();
        }
    }
}
