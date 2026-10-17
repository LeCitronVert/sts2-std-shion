using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionRageCharge() : ShionBerserkCard(2,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(3),
        new DynamicVar("BuffAmount", 5),
        new PatchUpVar(3),
        new CritDisplayTooltipVar()
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature))
        {
            await ShionMechanicsHelper.PatchUpCreature(card.DynamicVars[PatchUpVar.Key].IntValue, card.Owner.Creature);
        }
        else
        {
            await CreatureCmd.Damage(
                choiceContext,
                card.Owner.Creature,
                card.DynamicVars.HpLoss.BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this
            );
        }
        
        await PowerCmd.Apply<CritRateUp>(choiceContext, card.Owner.Creature, card.DynamicVars["BuffAmount"].BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<VigorPower>(choiceContext, card.Owner.Creature, card.DynamicVars["BuffAmount"].BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BuffAmount"].UpgradeValueBy(2);
    }
}