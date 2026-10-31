using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class CreativeSpark : SlalTheEndlessRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Starter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Flashback>()
    ];
    
    public override async Task BeforeCombatStart()
    {
        foreach (var cardModel in Owner.PlayerCombatState?.AllCards ?? [])
        {
            SlalMechanicsHelper.AddNovel(cardModel);
        }

        // Cards created before the first turn starts also need the tracker to gain Novel.
        await SlalMechanicsHelper.EnsureCreatureHasNovelTracker(Owner.Creature);
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        var novelPower = Owner.Creature.GetPower<NovelTracker>();
        if (card.Owner == Owner && novelPower != null && !novelPower.CardsUsedThisFight.Contains(card.Id))
        {
            SlalMechanicsHelper.AddNovel(card);
        }
        
        return Task.CompletedTask;
    }
    
    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        return SlalMechanicsHelper.EnsureCreatureHasNovelTracker(Owner.Creature);
    }


    public override RelicModel GetUpgradeReplacement() => ModelDb.Relic<CreativeFlow>();
}