using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalCartoonyDodge() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("RandomGain").WithMultiplier((card, _) => ((SlalCartoonyDodge)card).CountUniquePowers()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.Random>()];

    private int CountUniquePowers()
    {
        if (CombatState == null) return 0;
        return Owner.Creature.Powers
            .Where(p => p.IsVisible && (p.Type == PowerType.Buff || (IsUpgraded && p.Type == PowerType.Debuff)))
            .DistinctBy(p => p.GetType())
            .Count();
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var amount = card.CountUniquePowers();
        if (amount <= 0) return;

        await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, amount, card);
        await CreatureCmd.GainBlock(card.Owner.Creature, amount, ValueProp.Move, play);
    }

    protected override void OnUpgrade()
    {
    }
}
