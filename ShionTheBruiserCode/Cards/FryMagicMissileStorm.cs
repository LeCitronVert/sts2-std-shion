using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Cooldown 1. Deal 8 damage. After being played, this card is free to play until it is discarded.
/// </summary>
public class FryMagicMissileStorm() : FryCooldownCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override int CanonicalStarCost => 2;

    protected override int BaseCooldown => 1;

    protected override IEnumerable<DynamicVar> CooldownCardVars => [new DamageVar(8M, ValueProp.Move)];

    private bool _isFree;

    protected override async Task OnCooldownCardPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        _isFree = true;
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card == this && card.Pile?.Type == PileType.Discard) _isFree = false;
        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (base.TryModifyEnergyCostInCombat(card, originalCost, out modifiedCost)) return true;
        if (card != this || !_isFree) return false;
        modifiedCost = 0;
        return true;
    }

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (base.TryModifyStarCost(card, originalCost, out modifiedCost)) return true;
        if (card != this || !_isFree) return false;
        modifiedCost = 0;
        return true;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M); //todo : upgrade not specified
    }
}
