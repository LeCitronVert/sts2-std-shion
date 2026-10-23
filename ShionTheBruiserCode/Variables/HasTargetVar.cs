using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

/// <summary>
/// Display-only var: 1 while the card is being aimed at a target, 0 otherwise.
/// Lets target-dependent previews stay hidden until a target is chosen.
/// </summary>
public class HasTargetVar() : DynamicVar(Key, 0)
{
    public const string Key = "HasTarget";

    private bool _hasTarget;

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        _hasTarget = target != null;
        PreviewValue = _hasTarget ? 1 : 0;
    }

    protected override decimal GetBaseValueForIConvertible()
    {
        return _hasTarget ? 1 : 0;
    }
}
