using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Thorns. Gain 1 (2) Strength. Gain 1 energy next turn.
/// </summary>
public class FrySpikesOfVillainy() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<ThornsPower>(1M),
        new PowerVar<StrengthPower>(1M),
        new EnergyVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<ThornsPower>(),
        HoverTipFactory.ForEnergy(this),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await PowerCmd.Apply<ThornsPower>(choiceContext, creature, DynamicVars["ThornsPower"].BaseValue, creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, creature, DynamicVars.Strength.BaseValue, creature, this);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, creature, DynamicVars.Energy.BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1M);
    }
}
