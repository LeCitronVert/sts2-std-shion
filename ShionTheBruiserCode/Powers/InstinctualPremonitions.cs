using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class InstinctualPremonitions : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public const int BlockBonusAgainstAttacks = 10;
    public const int ArtifactBonusAgainstDebuffs = 1;
    public const int StatsBonusAgainstBuffs = 1;
    public const int VigorBonusAgainstBlock = 10;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BlockBonusAgainstAttacks", BlockBonusAgainstAttacks),
        new DynamicVar("ArtifactBonusAgainstDebuffs", ArtifactBonusAgainstDebuffs),
        new DynamicVar("StatsBonusAgainstBuffs", StatsBonusAgainstBuffs),
        new DynamicVar("VigorBonusAgainstBlock", VigorBonusAgainstBlock),
    ];

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        foreach (var enemy in CombatState.Enemies)
        {
            if (!enemy.IsEnemy) continue;
            if (enemy.Monster == null) continue;
            
            foreach (var intent in enemy.Monster.NextMove.Intents)
            {
                // Just in case 
                if (Amount <= 0) break;
                
                GainBonusBasedOnIntent(intent);
            }
        }
        
        return Task.CompletedTask;
    }
    
    private async void GainBonusBasedOnIntent(AbstractIntent intent)
    {
        switch (intent.IntentType)
        {
            case IntentType.Attack:
            case IntentType.DeathBlow:
                Flash();
                await CreatureCmd.GainBlock(
                    Owner,
                    new BlockVar(BlockBonusAgainstAttacks, ValueProp.Unpowered),
                    null
                );
                await PowerCmd.Decrement(this);
                break;
            case IntentType.Buff:
            case IntentType.Summon:
            case IntentType.Escape:
            case IntentType.Heal:
                Flash();
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, StatsBonusAgainstBuffs, Owner, null!);
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner, StatsBonusAgainstBuffs, Owner, null!);
                await PowerCmd.Decrement(this);
                break;
            case IntentType.Debuff:
            case IntentType.DebuffStrong:
            case IntentType.StatusCard:
            case IntentType.CardDebuff:
                Flash();
                await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Owner, ArtifactBonusAgainstDebuffs, Owner, null!);
                await PowerCmd.Decrement(this);
                break;
            case IntentType.Defend:
                Flash();
                await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Owner, VigorBonusAgainstBlock, Owner, null!);
                await PowerCmd.Decrement(this);
                break;
            case IntentType.Hidden:
            case IntentType.Sleep:
            case IntentType.Stun:
            case IntentType.Unknown:
            default:
                break;
        }
    }
}