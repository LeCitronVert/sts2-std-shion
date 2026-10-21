using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalTurnTheTables(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(2),
        new PowerVar<VigorPower>(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    protected override bool ShouldGlowGoldInternal => CombatState != null && Owner.Creature.HasPower<WeakPower>();

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        // If you are Weak, apply Weak and gain Vigor twice
        var repeats = card.Owner.Creature.HasPower<WeakPower>() ? 2 : 1;
        for (var i = 0; i < repeats; i++)
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                play.Target,
                DynamicVars["WeakPower"].BaseValue,
                card.Owner.Creature,
                card
            );

            var weakOnTargetAmount = play.Target.GetPowerAmount<WeakPower>();
            if (weakOnTargetAmount > 0)
            {
                await PowerCmd.Apply<VigorPower>(
                    choiceContext,
                    card.Owner.Creature,
                    DynamicVars["VigorPower"].BaseValue * weakOnTargetAmount,
                    card.Owner.Creature,
                    card
                );
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WeakPower"].UpgradeValueBy(1);
        DynamicVars["VigorPower"].UpgradeValueBy(1);
    }
}