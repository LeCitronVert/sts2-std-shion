using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Strife;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Cyan Chaos Emerald Burst (Emerald Power, 3 (2)-cost Power): play a copy of every Power you've played this fight
/// (Emerald Powers excluded; each play counts, Replays don't), then end your turn.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class EmeraldPowerWater() : EmeraldPowerCard(3, TargetType.Self)
{
    public override ChaosEmeraldColor Color => ChaosEmeraldColor.Cyan;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("Copies").WithMultiplier((card, _) => ((EmeraldPowerWater)card).PowersToCopy().Count),
    ];

    /// <summary>Every Power played this fight, in order (shared by OnPlay and the preview).</summary>
    private List<CardModel> PowersToCopy()
    {
        if (CombatState is null || !CombatManager.Instance.IsInProgress) return [];
        return CombatManager.Instance.History.CardPlaysStarted
            .Where(e => e.CardPlay.IsFirstInSeries)
            .Select(e => e.CardPlay.Card)
            .Where(c => c.Owner == Owner && c.Type == CardType.Power && c is not EmeraldPowerCard)
            .ToList();
    }

    protected override async Task OnEmeraldPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is not { } combatState) return;
        // Snapshot first: the copies' own plays are added to the history
        foreach (var power in PowersToCopy())
        {
            if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead) return;
            var copy = combatState.CloneCard(power);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
            await CardCmd.AutoPlay(choiceContext, copy, null);
        }
        if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead) return;
        PlayerCmd.EndTurn(Owner, false);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
