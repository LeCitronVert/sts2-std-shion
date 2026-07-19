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

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionInstinctualPremonitions() : ShionBerserkCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public const int BlockBonusAgainstAttacks = 10;
    public const int EnergyBonusAgainstOthers = 1;
    public const int VigorBonusAgainstBlock = 10;
    public const int CounterStanceAgainstDebuffs = 1;
    public const int CostReductionWhenBerserk = 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BlockBonusAgainstAttacks", BlockBonusAgainstAttacks),
        new DynamicVar("VigorBonusAgainstBlock", VigorBonusAgainstBlock),
        new EnergyVar("EnergyBonusAgainstOthers", EnergyBonusAgainstOthers),
        new DynamicVar("CounterStanceAgainstDebuffs", CounterStanceAgainstDebuffs),
        new EnergyVar("CostReductionWhenBerserk", CostReductionWhenBerserk)
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        
        foreach (var enemy in CombatState?.Enemies ?? [])
        {
            if (!enemy.IsEnemy) continue;
            if (enemy.Monster == null) continue;
            
            foreach (var intent in enemy.Monster.NextMove.Intents)
            {
                GainBonusBasedOnIntent(intent, card.Owner.Creature);
            }
        }
        
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
    
    private async void GainBonusBasedOnIntent(AbstractIntent intent, Creature owner)
    {
        if (owner.Player == null) return;
        
        switch (intent.IntentType)
        {
            case IntentType.Attack:
            case IntentType.DeathBlow:
                await CreatureCmd.GainBlock(
                    owner,
                    new BlockVar(BlockBonusAgainstAttacks, ValueProp.Move),
                    null
                );
                break;
            case IntentType.Debuff:
            case IntentType.DebuffStrong:
            case IntentType.StatusCard:
            case IntentType.CardDebuff:
                await PowerCmd.Apply<CounterStance>(new ThrowingPlayerChoiceContext(), owner, CounterStanceAgainstDebuffs, owner, this);
                break;
            case IntentType.Buff:
            case IntentType.Summon:
            case IntentType.Escape:
            case IntentType.Heal:
            case IntentType.Defend:
            case IntentType.Hidden:
            case IntentType.Sleep:
            case IntentType.Stun:
            case IntentType.Unknown:
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