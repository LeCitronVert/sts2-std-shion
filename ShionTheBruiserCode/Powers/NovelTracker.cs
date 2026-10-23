using MegaCrit.Sts2.Core.Entities.Creatures;
using Godot;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class NovelTracker : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.None;
    public override PowerStackType StackType => PowerStackType.None;
    protected override bool IsVisibleInternal => false;
    
    public List<ModelId> CardsUsedThisFight = [];

    // MemberwiseClone shares the list with the canonical model, so give each instance its own.
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        CardsUsedThisFight = [];
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner != Owner.Player) return Task.CompletedTask;
        
        // Only a triggered Novel locks identical cards out of Novel for the rest of the combat.
        if (card.Keywords.Contains(SlalCardKeywords.Novel))
        {
            if (!CardsUsedThisFight.Contains(card.Id)) CardsUsedThisFight.Add(card.Id);
            SlalMechanicsHelper.TriggerNovel(card, Owner);
        }

        return Task.CompletedTask;
    }
}
