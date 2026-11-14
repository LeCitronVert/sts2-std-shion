using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Singletons;

/// <summary>
/// Plays Seels' card animation (<see cref="SeelsTheRecklessCard.PlayAnimation"/>, "Cast" for Skills and Powers)
/// when he plays one of his cards, so the cards don't each have to trigger it.
/// Attacks animate through their damage command, and Blood Bend plays "Blood" (SeelsMechanicsHelper.BloodBend).
/// </summary>
public class SeelsAnimationSingleton() : CustomSingletonModel(HookType.Combat)
{
    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        await base.BeforeCardPlayed(cardPlay);
        if (cardPlay.Card is not SeelsTheRecklessCard { PlayAnimation: { } animation } card) return;
        if (card.Owner.Character is not SeelsTheReckless) return;

        await CreatureCmd.TriggerAnim(card.Owner.Creature, animation, card.Owner.Character.CastAnimDelay);
    }
}
