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
    
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner != Owner.Player) return Task.CompletedTask;
        
        if (!CardsUsedThisFight.Contains(card.Id)) CardsUsedThisFight.Add(card.Id);
        
        if (card.Keywords.Contains(SlalCardKeywords.Novel))
        {
            SlalMechanicsHelper.TriggerNovel(card, Owner);
        }

        return Task.CompletedTask;
    }
}
