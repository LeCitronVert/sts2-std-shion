using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionCrunch() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DebuffAmount", 1),
        new DynamicVar("VigorForVuln", 10),
        new DynamicVar("VigorForDebil", 15),
        new CriticalDrawVar(10)
    ];
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.HasVigor(this.Owner.Creature);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var owner = card.Owner.Creature;
        if (!ShionMechanicsHelper.HasVigor(owner)) return;
        var vigorAmount = owner.Powers.OfType<VigorPower>().First().Amount;
        ArgumentNullException.ThrowIfNull(play.Target);
        
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, card.DynamicVars["DebuffAmount"].BaseValue, card.Owner.Creature, card);
        if (vigorAmount >= DynamicVars["VigorForVuln"].BaseValue)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, card.DynamicVars["DebuffAmount"].BaseValue, card.Owner.Creature, card);
        }
        if (vigorAmount >= DynamicVars["VigorForDebil"].BaseValue)
        {
            await PowerCmd.Apply<DebilitatePower>(choiceContext, play.Target, card.DynamicVars["DebuffAmount"].BaseValue, card.Owner.Creature, card);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
    
    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
        
        return Task.CompletedTask;
    }
}