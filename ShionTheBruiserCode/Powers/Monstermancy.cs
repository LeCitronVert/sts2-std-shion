using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Monstermancy() : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<ShionMonsterSpell>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];


    private readonly Dictionary<Creature, MonsterMagicHelper> _learningMagic = new();

    private MonsterMagicHelper GetOrCreateHelper(Creature enemy)
    {
        if (_learningMagic.TryGetValue(enemy, out var helper)) return helper;
        helper = new MonsterMagicHelper(enemy);
        _learningMagic[enemy] = helper;
        return helper;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        var enemy = command.Attacker;

        if (enemy is { IsEnemy: true } && IsCopyable(command))
        {
            var totalDamage = 0;
            var hitCount = 0;

            foreach (var hits in command.Results.Where(IsRelevantDamageResults))
            {
                hitCount += hits.Count;
                totalDamage += hits.Sum(r => r.TotalDamage);
            }
            
            if (hitCount > 0)
            {
                var helper = GetOrCreateHelper(enemy);
                helper.HasAttack = true;
                helper.HitCount = hitCount;
                // Average per hit.
                helper.DamagePerHit = totalDamage / hitCount;
            }
        }

        await base.AfterAttack(choiceContext, command);
    }

    public override async Task AfterBlockGained(
        Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature.IsEnemy)
        {
            var helper = GetOrCreateHelper(creature);
            helper.HasBlock = true;
            helper.BlockAmount = (int)amount;
        }

        await base.AfterBlockGained(creature, amount, props, cardSource);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (applier is { IsEnemy: true } && MonsterPowersHelper.IsCopyable(power.GetType()))
        {
            var helper = GetOrCreateHelper(applier);
            if (power.Owner == applier)
                helper.PowersOnSelf.Add((power.GetType(), (int)amount));
            else
                helper.PowersOnTarget.Add((power.GetType(), (int)amount));
        }

        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        
        foreach (var helper in _learningMagic.Values)
        {
            var spell = CombatState.CreateCard<ShionMonsterSpell>(player);
            spell.Configure(helper);
            await CardPileCmd.AddGeneratedCardToCombat(
                spell,
                PileType.Hand,
                player
            );
        }
        _learningMagic.Clear();
    }

    private static bool IsCopyable(AttackCommand command) =>
        true; // todo

    private static bool IsCopyable(PowerModel power) =>
        true; // todo

    private bool IsRelevantDamageResults(List<DamageResult> results) => results.First().Receiver == Owner;
    
    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card is not ShionMonsterSpell || !ShionMechanicsHelper.IsBerserk(Owner))
        {
            return false;
        }

        modifiedCost -= 1;
        
        return true;
    }
    
    // Berserk Power stuff
    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner) return Task.CompletedTask;
        
        ChangePulse();
        return Task.CompletedTask;
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        ChangePulse();
        
        return Task.CompletedTask;
    }
    

    public void ChangePulse()
    {
        var shouldPulse = ShionMechanicsHelper.IsBerserk(Owner);

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