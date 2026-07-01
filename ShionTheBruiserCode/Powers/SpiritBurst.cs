using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class SpiritBurst: ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    private const int _infiniteAutoPlayCap = 9;
    protected override object InitInternalData() => new Data();
    
    public override async Task AfterCardDrawnEarly(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        var power = this;
        Data? data;
        if (card.Owner.Creature != power.Owner || !card.Tags.Contains(ShionCardTags.Energy))
            data = null;
        else
        {
            data = power.GetInternalData<Data>();
            var flag = true;
            
            if (power.Owner.CombatState.HittableEnemies.All<Creature>((Func<Creature, bool>) (c => c.HpDisplay.IsInfinite())))
            {
                if (data.infiniteAutoPlaysThisTurn >= 9)
                {
                    flag = false;
                    if (!data.showedCapReachedMessage)
                    {
                        ThinkCmd.Play(new LocString("powers", "SHIONTHEBRUISER-SPIRIT_BURST.autocap"), power.Owner);
                        data.showedCapReachedMessage = true;
                    }
                }
                ++data.infiniteAutoPlaysThisTurn;
            }
            else
                power.ResetInfiniteAutoPlayData();
            if (!flag)
            {
                data = null;
            }
            else
            {
                data.autoPlayingCards.Add(card);
                await CardCmd.AutoPlay(choiceContext, card, null);
                data.autoPlayingCards.Remove(card);
                await CardPileCmd.Draw(choiceContext, 1, card.Owner);
                data = null;
            }
        }
    }
    
    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains<Creature>(this.Owner))
            return Task.CompletedTask;
        this.ResetInfiniteAutoPlayData();
        return Task.CompletedTask;
    }
    
    public override Task BeforeAttack(AttackCommand command)
    {
        if (!((IEnumerable<AbstractModel>) this.GetInternalData<SpiritBurst.Data>().autoPlayingCards).Contains<AbstractModel>(command.ModelSource))
            return Task.CompletedTask;
        command.WithHitFx("vfx/hellraiser_attack_vfx", command.HitSfx, command.TmpHitSfx).WithAttackerAnim("Cast", command.Attacker.Player.Character.CastAnimDelay).SpawningHitVfxOnEachCreature().WithHitVfxSpawnedAtBase();
        return Task.CompletedTask;
    }
    
    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        var power = this;
        if (!participants.Contains<Creature>(power.Owner))
            return;
        if (power.Amount > 1)
        {
            await PowerCmd.Decrement((PowerModel) power);
        }
        else
        {
            power.Flash();
            await Cmd.CustomScaledWait(0.2f, 0.4f);
            await PowerCmd.Remove((PowerModel) power);
            await PowerCmd.Apply<NoBlockPower>(
                choiceContext,
                power.Owner,
                999,
                power.Owner,
                null
            );
        }
    }
    
    private void ResetInfiniteAutoPlayData()
    {
        SpiritBurst.Data internalData = this.GetInternalData<Data>();
        internalData.infiniteAutoPlaysThisTurn = 0;
        internalData.showedCapReachedMessage = false;
    }
    private class Data
    {
        public readonly HashSet<CardModel> autoPlayingCards = new HashSet<CardModel>();
        public int infiniteAutoPlaysThisTurn;
        public bool showedCapReachedMessage;
    }
}