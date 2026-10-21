using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Very Slimy. Deal 4 damage to ALL enemies.
/// Each time you create a Slimed, this card's damage increases by 1 (2) this combat.
/// The Slimed created by its own Very Slimy are created after the damage is dealt.
/// </summary>
public class SlalSlimeRain(): SlalSlimyCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AllEnemies)
{
    private decimal _extraDamageFromSlimed;

    /// <summary>
    /// Required so we can restore the extra damage amount after a downgrade (see Rampage).
    /// </summary>
    private decimal ExtraDamageFromSlimed
    {
        get => _extraDamageFromSlimed;
        set
        {
            AssertMutable();
            _extraDamageFromSlimed = value;
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.VerySlimy];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4M, ValueProp.Move),
        new DynamicVar("Increase", 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Slimed>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card is not Slimed || card.Owner != Owner) return Task.CompletedTask;

        var increase = DynamicVars["Increase"].BaseValue;
        DynamicVars.Damage.BaseValue += increase;
        ExtraDamageFromSlimed += increase;
        return Task.CompletedTask;
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Damage.BaseValue += ExtraDamageFromSlimed;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(1);
    }
}
