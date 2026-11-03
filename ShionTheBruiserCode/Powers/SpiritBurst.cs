using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// For X turns, your Strikes and Energy cards cost 0, and playing an Energy card draws 1 card.
/// When it ends, you can no longer gain Block from cards or heal this combat.
/// (Used to auto-play Strikes and Energy cards when drawn; changed so other players can keep up in multiplayer.)
/// </summary>
public class SpiritBurst : ShionTheBruiserPower
{
    public const int CardsDrawn = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<NoBlockPower>(),
        HoverTipFactory.FromPower<NoHealPower>(),
    ];

    protected override object InitInternalData() => new Data();

    private bool IsBoosted(CardModel card) =>
        card.Owner.Creature == Owner
        && (card.Tags.Contains(ShionCardTags.Energy) || card.Tags.Contains(CardTag.Strike));

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!IsBoosted(card) || card.EnergyCost.CostsX) return false;

        modifiedCost = 0M;
        return true;
    }

    // Visuals only: the cards played during Spirit Burst keep its lightning and strike effect.
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (!IsBoosted(cardPlay.Card)) return Task.CompletedTask;

        GetInternalData<Data>().playingCards.Add(cardPlay.Card);
        ShionVfx.SetSpiritBurstAutoPlaying(Owner, true);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (GetInternalData<Data>().playingCards.Remove(card))
        {
            ShionVfx.SetSpiritBurstAutoPlaying(Owner, false);
        }

        if (card.Owner.Creature != Owner || !card.Tags.Contains(ShionCardTags.Energy)) return;

        Flash();
        await CardPileCmd.Draw(choiceContext, CardsDrawn, card.Owner);
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.ModelSource is not CardModel card || command.Attacker?.Player == null
            || !GetInternalData<Data>().playingCards.Contains(card))
            return Task.CompletedTask;
        command.WithHitFx(null, command.HitSfx, command.TmpHitSfx)
            .WithHitVfxNode(ShionVfx.CreateSpiritBurstStrike)
            .WithAttackerAnim("Cast", command.Attacker.Player.Character.CastAnimDelay);
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
            return;
        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
        }
        else
        {
            Flash();
            await Cmd.CustomScaledWait(0.2f, 0.4f);
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<NoBlockPower>(choiceContext, Owner, 999, Owner, null);
            await PowerCmd.Apply<NoHealPower>(choiceContext, Owner, 999, Owner, null);
        }
    }

    private class Data
    {
        public readonly HashSet<CardModel> playingCards = new();
    }
}
