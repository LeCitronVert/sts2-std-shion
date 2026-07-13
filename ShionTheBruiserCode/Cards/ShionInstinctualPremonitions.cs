using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionInstinctualPremonitions() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
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
            case IntentType.Buff:
            case IntentType.Summon:
            case IntentType.Escape:
            case IntentType.Heal:
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), owner, StatsBonusAgainstBuffs, owner, null!);
                await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), owner, StatsBonusAgainstBuffs, owner, null!);
                break;
            case IntentType.Debuff:
            case IntentType.DebuffStrong:
            case IntentType.StatusCard:
            case IntentType.CardDebuff:
                await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), owner, ArtifactBonusAgainstDebuffs, owner, null!);
                break;
            case IntentType.Defend:
                await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), owner, VigorBonusAgainstBlock, owner, null!);
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