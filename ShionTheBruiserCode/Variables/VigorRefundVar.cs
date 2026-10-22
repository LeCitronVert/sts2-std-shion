using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

/// <summary>
/// Display-only var showing how much Vigor next turn Energy Ball will give if played right now.
/// </summary>
public class VigorRefundVar() : DynamicVar(Key, 0)
{
    public const string Key = "VigorRefund";

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        PreviewValue = CombatManager.Instance.IsInProgress && card.CombatState != null
            ? ShionEnergyBall.GetVigorRefund(card)
            : 0;
    }
}
