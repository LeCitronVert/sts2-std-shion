using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 8 Block. Gain 1 (2) Dexterity. Gain 1 Weak.
/// </summary>
public class SlalBlooper() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8M, ValueProp.Move),
        new PowerVar<DexterityPower>(1),
        new PowerVar<WeakPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<WeakPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
        await PowerCmd.Apply<DexterityPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Dexterity.BaseValue, card.Owner.Creature, card);
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Dexterity.UpgradeValueBy(1);
    }
}
