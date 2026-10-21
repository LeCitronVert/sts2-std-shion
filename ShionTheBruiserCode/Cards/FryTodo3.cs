using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// ALL players gain 1 energy and 1 (2) energy next turn.
/// </summary>
public class FryTodo3() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AllAllies)
{
    public override int CanonicalStarCost => 1;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        new DynamicVar("NextTurnEnergy", 1M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;
        foreach (var player in CombatState.GetTeammatesOf(Owner.Creature).Where(c => c.IsPlayer && c.IsAlive).ToList())
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player.Player!);
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, player, DynamicVars["NextTurnEnergy"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        UpgradeStarCostBy(-1);
        DynamicVars["NextTurnEnergy"].UpgradeValueBy(1M);
    }
}
