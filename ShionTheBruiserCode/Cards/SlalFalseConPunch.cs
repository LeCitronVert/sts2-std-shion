using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalFalseConPunch() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.RandomEnemy)
{
    private const string _increaseKey = "Scale";
    private Decimal _extraDamageFromPlays;

    /// <summary>
    /// Required so we can restore the extra damage amount after a downgrade (ie Magiknight)
    /// @see Rampage
    /// </summary>
    private Decimal ExtraDamageFromPlays
    {
        get => this._extraDamageFromPlays;
        set
        {
            this.AssertMutable();
            this._extraDamageFromPlays = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<Confusion>(4),
        new RepeatVar(2),
        new DynamicVar("Scale", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Confusion>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        for (int i = 0; i < DynamicVars["Repeat"].BaseValue; i++)
        {
            Creature enemy = card.Owner.RunState.Rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>) CombatState.HittableEnemies);
            if (enemy != null)
            {
                await PowerCmd.Apply<Confusion>(
                    choiceContext,
                    enemy,
                    DynamicVars["Confusion"].BaseValue,
                    card.Owner.Creature,
                    card
                );
            }
            enemy = (Creature) null;
        }

        DynamicVars.Repeat.BaseValue += card.DynamicVars["Scale"].BaseValue;
        card.ExtraDamageFromPlays += card.DynamicVars["Scale"].BaseValue;
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Repeat.BaseValue += ExtraDamageFromPlays;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Scale"].UpgradeValueBy(1);
    }
}