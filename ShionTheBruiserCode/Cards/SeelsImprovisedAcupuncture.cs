using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Lose 1 HP. Draw as many cards (+1). Increase this card's HP loss by 1 this combat.
/// </summary>
public class SeelsImprovisedAcupuncture() : SeelsTheRecklessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(1M),
        new DynamicVar("ExtraDraw", 0M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hpLoss = DynamicVars.HpLoss.BaseValue;
        await SeelsMechanicsHelper.LoseHp(choiceContext, Owner.Creature, hpLoss, this);
        await CardPileCmd.Draw(choiceContext, hpLoss + DynamicVars["ExtraDraw"].BaseValue, Owner);
        DynamicVars.HpLoss.BaseValue += 1;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ExtraDraw"].UpgradeValueBy(1M);
    }
}
