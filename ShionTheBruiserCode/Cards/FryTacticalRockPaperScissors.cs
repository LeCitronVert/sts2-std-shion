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
/// If the enemy intends to attack, gain 1 (2) Dexterity. If it intends to defend, gain 1 (2) Strength.
/// If it intends neither, gain 1 (2) energy next turn. Exhaust.
/// </summary>
public class FryTacticalRockPaperScissors() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Amount", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var creature = Owner.Creature;
        var amount = DynamicVars["Amount"].BaseValue;
        var intents = play.Target.Monster?.NextMove.Intents.Select(i => i.IntentType).ToList() ?? [];

        var attacks = FryMechanicsHelper.IntendsToAttack(play.Target);
        var defends = intents.Contains(IntentType.Defend);

        if (attacks)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, creature, amount, creature, this);
        }
        if (defends)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, creature, amount, creature, this);
        }
        if (!attacks && !defends)
        {
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, creature, amount, creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Amount"].UpgradeValueBy(1M);
    }
}
