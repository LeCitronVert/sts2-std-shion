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
/// SkeleBot 9000 (Team-Up), on Fry: whenever a Partner Summons X, both Corrupted Punch Constructs gain X Blocking intents.
/// Whenever you spend X Stars, they gain X Attacking intents. (Times Amount.)
/// </summary>
public class Skelebot9000 : TeamUpPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<CorruptedPunchConstructPower>()];

    private IEnumerable<CorruptedPunchConstructPower> Constructs()
    {
        foreach (var creature in Partners.Prepend(Owner))
        {
            if (creature.Player is not { } player) continue;
            if (CorruptedPunchConstruct.GetConstruct(player)?.GetPower<CorruptedPunchConstructPower>() is { } power) yield return power;
        }
    }

    public override Task AfterSummon(PlayerChoiceContext choiceContext, Player summoner, decimal amount)
    {
        if (!LivingPartners.Contains(summoner.Creature)) return Task.CompletedTask;

        var blocks = (int)amount * Amount;
        foreach (var construct in Constructs().ToList())
        {
            construct.AddBlocks(blocks);
        }
        return Task.CompletedTask;
    }

    public override Task AfterStarsSpent(int amount, Player spender)
    {
        if (amount <= 0 || spender != Owner.Player) return Task.CompletedTask;

        var attacks = amount * Amount;
        foreach (var construct in Constructs().ToList())
        {
            construct.AddAttacks(attacks);
        }
        return Task.CompletedTask;
    }
}
