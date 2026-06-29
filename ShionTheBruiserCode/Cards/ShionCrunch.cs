using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionCrunch() : ShionTheBruiserCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DebuffAmount", 1),
        new DynamicVar("VigorForVuln", 15),
        new DynamicVar("VigorForDebil", 30)
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
}