using ShionTheBruiser.ShionTheBruiserCode.Enums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Whenever you play a Blood Bend card, lose 1 HP after it resolves. Whenever you lose HP, gain 1 Blood.
/// </summary>
public class ThornBracelet : SeelsTheRecklessRelic, ISeelsListener
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Blood", 1M),
        new HpLossVar(1M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
    ];

    /// <summary>Cards that Blood Bent during their current play.</summary>
    private HashSet<CardModel> _bloodBentCards = [];

    // MemberwiseClone shares the set with the canonical model, so give each instance its own.
    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _bloodBentCards = [];
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature || result.UnblockedDamage <= 0) return;
        Flash();
        await SeelsMechanicsHelper.GainBlood(choiceContext, Owner.Creature, DynamicVars["Blood"].BaseValue);
    }

    public Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source)
    {
        if (player == Owner && source is not null && source.Owner == Owner) _bloodBentCards.Add(source);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_bloodBentCards.Remove(cardPlay.Card)) return;
        Flash();
        await SeelsMechanicsHelper.LoseHp(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, null);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _bloodBentCards.Clear();
        return Task.CompletedTask;
    }

    public override RelicModel GetUpgradeReplacement() => ModelDb.Relic<ThornBracers>();
}
