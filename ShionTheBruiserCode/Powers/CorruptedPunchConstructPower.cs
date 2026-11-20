using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Monsters;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// On a Corrupted Punch Construct (SkeleBot 9000): takes the unblocked attack damage meant for its owner (and their Osty).
/// At the end of its owner's turn it acts: each Attacking intent deals damage to the frontmost enemy,
/// each Blocking intent gives its owner Block. It starts each turn with 1 of each; SkeleBot 9000 adds more.
/// </summary>
public class CorruptedPunchConstructPower : ShionTheBruiserPower
{
    public const int DamagePerAttack = 5; //todo : damage not specified
    public const int BlockPerBlock = 5; //todo : block not specified

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Attacks", 1M),
        new DynamicVar("Blocks", 1M),
        new DamageVar(DamagePerAttack, ValueProp.Move),
        new BlockVar(BlockPerBlock, ValueProp.Unpowered),
    ];

    public int Attacks
    {
        get => DynamicVars["Attacks"].IntValue;
        private set => DynamicVars["Attacks"].BaseValue = value;
    }

    public int Blocks
    {
        get => DynamicVars["Blocks"].IntValue;
        private set => DynamicVars["Blocks"].BaseValue = value;
    }

    public void AddAttacks(int amount)
    {
        if (amount <= 0) return;
        Attacks += amount;
        Flash();
    }

    public void AddBlocks(int amount)
    {
        if (amount <= 0) return;
        Blocks += amount;
        Flash();
    }

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (Owner.IsDead || !props.IsPoweredAttack()) return target;
        if (Owner.PetOwner is not { } owner) return target;

        return target == owner.Creature || target == owner.Osty ? Owner : target;
    }

    // It can't be targeted with powers once dead.
    public override bool ShouldAllowHitting(Creature creature)
    {
        return creature != Owner || creature.IsAlive;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (Owner.IsDead || Owner.PetOwner is not { } owner || side != owner.Creature.Side) return;

        for (var i = 0; i < Attacks && Owner.IsAlive; i++)
        {
            var target = Owner.CombatState?.HittableEnemies.FirstOrDefault();
            if (target is null) break;
            await CreatureCmd.TriggerAnim(Owner, "Attack", 0.25f);
            await CreatureCmd.Damage(choiceContext, target, DamagePerAttack, ValueProp.Move, Owner);
        }

        for (var i = 0; i < Blocks && Owner.IsAlive && owner.Creature.IsAlive; i++)
        {
            await CreatureCmd.GainBlock(owner.Creature, BlockPerBlock, ValueProp.Unpowered, null);
        }
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner.PetOwner == player)
        {
            Attacks = 1;
            Blocks = 1;
        }
        return Task.CompletedTask;
    }
}
