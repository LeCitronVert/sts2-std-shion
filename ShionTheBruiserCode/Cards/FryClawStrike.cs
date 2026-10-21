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
/// Deal 8 (10) damage. Increase the damage of ALL Claw cards by 2 (3) this combat.
/// Includes Claw Strike and the Defect's Claw.
/// </summary>
public class FryClawStrike() : FryTheCommanderCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    private static readonly System.Reflection.MethodInfo? BuffClaw =
        HarmonyLib.AccessTools.Method(typeof(Claw), "BuffFromClawPlay");

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8M, ValueProp.Move),
        new DynamicVar("Increase", 2M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Claw>()];

    private decimal _extraDamage;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var increase = DynamicVars["Increase"].BaseValue;
        foreach (var card in SeelsMechanicsHelper.GetAllCombatCards(Owner).ToList())
        {
            switch (card)
            {
                case FryClawStrike clawStrike:
                    clawStrike.Buff(increase);
                    break;
                case Claw claw:
                    BuffClaw?.Invoke(claw, [increase]);
                    break;
            }
        }
    }

    private void Buff(decimal amount)
    {
        DynamicVars.Damage.BaseValue += amount;
        _extraDamage += amount;
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Damage.BaseValue += _extraDamage;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
        DynamicVars["Increase"].UpgradeValueBy(1M);
    }
}
