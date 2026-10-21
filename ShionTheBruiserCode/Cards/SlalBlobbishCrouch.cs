using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Slimy. Gain 5 (7) Block and 1 Weak.
/// </summary>
public class SlalBlobbishCrouch(): SlalSlimyCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5M, ValueProp.Move),
        new PowerVar<WeakPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2M);
    }
}
