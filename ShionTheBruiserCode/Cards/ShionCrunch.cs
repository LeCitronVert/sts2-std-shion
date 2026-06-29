using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

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

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var vigorAmount = card.Owner.Creature.Powers.FirstOrDefault(p => p is VulnerablePower)?.Amount ?? 0;
        if (vigorAmount == 0) return;
        ArgumentNullException.ThrowIfNull(play.Target, "play.Target");
        
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