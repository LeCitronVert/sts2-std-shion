#if STS2_BETA
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.FoolArrows;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Fool-ow Up Attack (Team-Up), on Shion. The next time your Partner Shoots an Arrow, launch an Energy Attack at the
/// enemy it hit, with the same base damage as the card that shot it (once per stack). Then this power goes away.
/// The attack comes from a <see cref="Cards.ShionFoolowUpBlast"/> owned by Shion: an Energy card, so Shion's attack and Energy modifiers apply.
/// Beta only.
/// </summary>
public class FoolowUpAttack : TeamUpPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Base damage of the Arrow the Partner just shot, until it lands.</summary>
    private decimal? _pendingDamage;

    public override Task AfterArrowShot(Player owner, ArrowSlot? arrow, PlayerChoiceContext context, CardModel? source)
    {
        if (arrow != null && Partners.Contains(owner.Creature))
        {
            _pendingDamage = FoolTeamUpHelper.GetBaseDamage(source);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterArrowImpaled(ImpaledPower power, ArrowSlot arrow, PlayerChoiceContext context, CardModel? source)
    {
        if (_pendingDamage == null || !Partners.Contains(arrow.owner.Creature)) return;
        await Launch(context, power.Owner);
    }

    // The Arrow didn't land (its target died first): launch at a random enemy once the Partner's card is done.
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_pendingDamage == null || !Partners.Contains(cardPlay.Card.Owner.Creature)) return;
        await Launch(choiceContext, null);
    }

    private async Task Launch(PlayerChoiceContext choiceContext, Creature? target)
    {
        var damage = _pendingDamage ?? 0m;
        _pendingDamage = null;
        var times = Amount;
        await PowerCmd.Remove(this);

        if (Owner.IsDead || Owner.Player is not { } shion || Owner.CombatState is not { } combatState || damage <= 0) return;

        // The blast comes from an Energy card of Shion's, so it counts as an Energy Attack.
        var card = combatState.CreateCard<ShionFoolowUpBlast>(shion);
        card.SetDamage(damage);

        for (var i = 0; i < times; i++)
        {
            var aimed = FoolTeamUpHelper.TargetOrRandom(shion, target);
            if (aimed == null) return;

            await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
                .FromCard(card, null)
                .Targeting(aimed)
                .WithEnergyBlast(card, "vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }
}
#endif
