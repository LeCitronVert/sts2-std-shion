using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionSkybreakerDance() : ShionTheBruiserCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1, ValueProp.Unblockable | ValueProp.Unpowered),
        new DynamicVar("VigorCap", 15),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var vigorPowerAmount = card.Owner.Creature.Powers.FirstOrDefault(p => p is VigorPower)?.DynamicVars["VigorPower"].BaseValue ?? 0;
        vigorPowerAmount = Math.Min(vigorPowerAmount, card.DynamicVars["VigorCap"].BaseValue);
        ArgumentNullException.ThrowIfNull(play.Target, "play.Target");
        
        for (int i = 0; i < vigorPowerAmount; i++)
        {
            await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
                .FromCard(card)
                .Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Unpowered()
                .Execute(choiceContext)
            ;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VigorCap"].UpgradeValueBy(15);
    }
}