using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionCrunch() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DebuffAmount", 1),
        new DynamicVar("VigorNeeded", 5),
        new CriticalDrawVar(10)
    ];
    
    protected override bool ShouldGlowGoldInternal => Owner.Creature.GetPowerAmount<VigorPower>() >= DynamicVars["VigorNeeded"].BaseValue;
    protected override bool ShouldGlowRedInternal => !ShouldGlowGoldInternal;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var owner = card.Owner.Creature;

        if (owner.GetPowerAmount<VigorPower>() < card.DynamicVars["VigorNeeded"].BaseValue)
        {
            await CardCmd.Exhaust(choiceContext, card);
            return;
        }
        
        ArgumentNullException.ThrowIfNull(play.Target);
        
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, card.DynamicVars["DebuffAmount"].BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, card.DynamicVars["DebuffAmount"].BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<DebilitatePower>(choiceContext, play.Target, card.DynamicVars["DebuffAmount"].BaseValue, card.Owner.Creature, card);
        
        await PowerCmd.Apply<VigorPower>(choiceContext, card.Owner.Creature, -card.DynamicVars["VigorNeeded"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DebuffAmount"].UpgradeValueBy(1);
    }
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
    }
    
    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType pileTypeForCardPlay = base.GetResultPileTypeForCardPlay();
        return pileTypeForCardPlay != PileType.Discard ? pileTypeForCardPlay : PileType.Hand;
    }
}