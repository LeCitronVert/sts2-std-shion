using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionInstinctualPremonitions() : ShionBerserkCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public const int BlockBonusAgainstAttacks = 10;
    public const int EnergyBonusAgainstOthers = 1;
    public const int VigorBonusAgainstBlock = 10;
    public const int CounterStrategyAgainstDebuffs = 1;
    public const int CostReductionWhenBerserk = 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BlockBonusAgainstAttacks", BlockBonusAgainstAttacks),
        new DynamicVar("VigorBonusAgainstBlock", VigorBonusAgainstBlock),
        new EnergyVar("EnergyBonusAgainstOthers", EnergyBonusAgainstOthers),
        new DynamicVar("CounterStrategyAgainstDebuffs", CounterStrategyAgainstDebuffs),
        new EnergyVar("CostReductionWhenBerserk", CostReductionWhenBerserk),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("PremonitionBlock").WithMultiplier((card, _) =>
            CountIntents(card, IntentCategory.Attack) * PreviewHelper.ModifiedBlock(card, BlockBonusAgainstAttacks, ValueProp.Move)),
        new CalculatedVar("PremonitionCounterStrategy").WithMultiplier((card, _) =>
            CountIntents(card, IntentCategory.Debuff) * CounterStrategyAgainstDebuffs),
        new CalculatedVar("PremonitionEnergy").WithMultiplier((card, _) =>
            CountIntents(card, IntentCategory.Other) * EnergyBonusAgainstOthers),
    ];

    internal enum IntentCategory { Attack, Debuff, Other }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<CounterStrategy>(),
        HoverTipFactory.ForEnergy(this),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        // The cost reduction is a Modify hook, so the Berserk trigger is marked here
        ShionMechanicsHelper.TriggerBerserk(card.Owner.Creature);
        
        foreach (var intent in GetEnemyIntents(card))
        {
            await GainBonusBasedOnIntent(intent, card.Owner.Creature);
        }
    }

    private static List<AbstractIntent> GetEnemyIntents(CardModel card) => GetEnemyIntents(card.CombatState);

    // Also used by the Monster Orb (Watch and Learn)
    internal static List<AbstractIntent> GetEnemyIntents(ICombatState? combatState)
    {
        List<AbstractIntent> intents = [];
        foreach (var enemy in combatState?.Enemies ?? [])
        {
            if (!enemy.IsEnemy) continue;
            if (enemy.Monster == null) continue;

            intents.AddRange(enemy.Monster.NextMove.Intents);
        }
        return intents;
    }

    internal static IntentCategory Categorize(AbstractIntent intent)
    {
        switch (intent.IntentType)
        {
            case IntentType.Attack:
            case IntentType.DeathBlow:
                return IntentCategory.Attack;
            case IntentType.Debuff:
            case IntentType.DebuffStrong:
            case IntentType.StatusCard:
            case IntentType.CardDebuff:
                return IntentCategory.Debuff;
            default:
                return IntentCategory.Other;
        }
    }

    private static int CountIntents(CardModel card, IntentCategory category) =>
        GetEnemyIntents(card).Count(i => Categorize(i) == category);

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
    
    private async Task GainBonusBasedOnIntent(AbstractIntent intent, Creature owner)
    {
        if (owner.Player == null) return;
        
        switch (Categorize(intent))
        {
            case IntentCategory.Attack:
                await CreatureCmd.GainBlock(
                    owner,
                    new BlockVar(BlockBonusAgainstAttacks, ValueProp.Move),
                    null
                );
                break;
            case IntentCategory.Debuff:
                await PowerCmd.Apply<CounterStrategy>(new ThrowingPlayerChoiceContext(), owner, CounterStrategyAgainstDebuffs, owner, this);
                break;
            case IntentCategory.Other:
            default:
                await PlayerCmd.GainEnergy(EnergyBonusAgainstOthers, owner.Player);
                break;
        }
    }
    
    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        Decimal originalCost,
        out Decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card != this || !ShionMechanicsHelper.IsBerserk(Owner.Creature))
        {
            return false;
        }

        modifiedCost -= 1;
        
        return true;
    }
}