using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalBurstOfLaughter() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(1),
        new EnergyVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.ForEnergy(this),
        HoverTipFactory.FromPower<Confusion>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<WeakPower>(choiceContext, card.Owner.Creature, card.DynamicVars.Weak.BaseValue, card.Owner.Creature, card);
        await PlayerCmd.GainEnergy(card.DynamicVars.Energy.BaseValue, card.Owner);
        await PowerCmd.Apply<BurstOfLaughter>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}
