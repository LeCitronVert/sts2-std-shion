using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionChaosCrash() : ShionBerserkCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12M, ValueProp.Move),
        new HpLossVar(3M),
        new PatchUpVar(3)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        if (ShionMechanicsHelper.TriggerBerserk(card.Owner.Creature))
        {
            await ShionMechanicsHelper.PatchUpCreature(card.DynamicVars[PatchUpVar.Key].IntValue, card.Owner.Creature);
        }
        else
        {
            await CreatureCmd.Damage(choiceContext, card.Owner.Creature, card.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, card);
        }
        
        if (card.CombatState != null)
            await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue).FromCard(card)
                .TargetingAllOpponents(card.CombatState).WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
                .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4M);
    }
}