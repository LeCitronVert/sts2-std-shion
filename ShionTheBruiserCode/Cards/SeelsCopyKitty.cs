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
/// Play a copy of ALL the cards another player played this turn and Exhaust them.
/// </summary>
public class SeelsCopyKitty() : SeelsTheRecklessCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target?.Player);
        if (CombatState is null) return;

        // Each card once (not its replays), in the order they were played.
        var played = CombatManager.Instance.History.CardPlaysFinished
            .Where(e => e.HappenedThisTurn(CombatState)
                        && e.CardPlay.Card.Owner == play.Target.Player
                        && e.CardPlay.IsFirstInSeries)
            .Select(e => e.CardPlay.Card)
            .ToList();

        foreach (var original in played)
        {
            if (CombatState is null || Owner.Creature.IsDead) return;
            var copy = CombatState.CreateCard(original.CanonicalInstance, Owner);
            if (original.IsUpgraded && copy.IsUpgradable) CardCmd.Upgrade(copy);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
            copy.ExhaustOnNextPlay = true;
            await CardCmd.AutoPlay(choiceContext, copy, null);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
