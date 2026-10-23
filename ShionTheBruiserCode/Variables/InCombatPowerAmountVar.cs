using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

/// <summary>
/// Display-only var: the amount of <typeparamref name="TPower"/> the card's owner has in combat, 0 otherwise.
/// Reads the previewed card rather than storing one, since vars are cloned along with their card.
/// </summary>
public class InCombatPowerAmountVar<TPower>(string key) : DynamicVar(key, 0) where TPower : PowerModel
{
    private decimal _amount;

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        _amount = card.CombatState != null && card.Owner?.Creature != null
            ? card.Owner.Creature.GetPowerAmount<TPower>()
            : 0;
        PreviewValue = _amount;
    }

    protected override decimal GetBaseValueForIConvertible()
    {
        return _amount;
    }
}
