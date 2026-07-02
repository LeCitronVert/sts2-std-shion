using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEepy() : ShionTheBruiserCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(20),
        new DynamicVar("HpGate", 33)
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust,
    ];


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.Heal(card.Owner.Creature, card.DynamicVars.Heal.BaseValue);
        PlayerCmd.EndTurn(card.Owner, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(10);
    }
    
    // Can only be played if you have less than 50% your max HP
    protected override bool IsPlayable
    {
        get
        {
            var missingHpRatio = (Owner.Creature.MaxHp - Owner.Creature.CurrentHp)
                                 / (float)Owner.Creature.MaxHp
            ;
            var hpGate = DynamicVars["HpGate"].BaseValue / 100M;
            
            return missingHpRatio >= (float) hpGate;
        }
    }
}