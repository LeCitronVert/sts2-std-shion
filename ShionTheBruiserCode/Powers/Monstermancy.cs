using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Monstermancy() : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

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
        if (applier is { IsEnemy: true } && IsCopyable(power))
        {
            var helper = GetOrCreateHelper(applier);

            if (power.Owner == applier)
                helper.PowersOnSelf.Add((power, (int)amount));
            else
                helper.PowersOnTarget.Add((power, (int)amount));
        }

        await base.AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource);
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        
        foreach (var helper in _learningMagic.Values)
        {
            var spell = new ShionMonsterSpell();
            spell.Configure(helper);
            
            spell.Owner = player;
            await CardPileCmd.Add(spell, PileType.Hand.GetPile(player));
        }
        _learningMagic.Clear();
    }

    private static bool IsCopyable(AttackCommand command) =>
        true; // todo

    private static bool IsCopyable(PowerModel power) =>
        true; // todo

    private bool IsRelevantDamageResults(List<DamageResult> results) => results.First().Receiver == Owner;
}