using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 3 (4) damage. Increase the damage of ALL Scratch cards by 1 (2) this combat. Add a copy of this card into your Draw Pile.
/// </summary>
public class SeelsScratch() : SeelsTheRecklessCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3M, ValueProp.Move),
        new DynamicVar("Increase", 1M),
    ];

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
        foreach (var scratch in SeelsMechanicsHelper.GetAllCombatCards(Owner).OfType<SeelsScratch>().ToList())
        {
            scratch.Buff(increase);
        }

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CreateClone(), PileType.Draw, Owner, CardPilePosition.Random));
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
        DynamicVars.Damage.UpgradeValueBy(1M);
        DynamicVars["Increase"].UpgradeValueBy(1M);
    }
}
