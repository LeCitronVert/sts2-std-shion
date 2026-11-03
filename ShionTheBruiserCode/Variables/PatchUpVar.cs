using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

public class PatchUpVar: DynamicVar
{
    public const string Key = "PatchUp";
    private const string TipKey = "SHIONTHEBRUISER-PATCH_UP";
    private const string TipTable = "static_hover_tips";

    public PatchUpVar(decimal patchUpValue) : base(Key, patchUpValue)
    {
        // Same as BaseLib's WithTooltip, plus the owner's current Patch Up limit.
        // Receives the displayed (possibly cloned) var, so read the owner from it, not from this instance.
        DynamicVarExtensions.DynamicVarTips[this] = locVar =>
        {
            var title = new LocString(TipTable, TipKey + ".title");
            var description = new LocString(TipTable, TipKey + ".description");
            title.Add(locVar);
            description.Add(locVar);

            var creature = GetOwnerCreature(locVar);
            var inCombat = creature?.CombatState != null && CombatManager.Instance.IsInProgress;
            var threshold = inCombat
                ? ShionMechanicsHelper.GetPatchUpThreshold(creature!)
                : ShionMechanicsHelper.BasePatchUpThreshold;
            description.Add("PatchUpLimit", Math.Round(threshold * 100m));
            description.Add("PatchUpInCombat", inCombat ? 1 : 0);

            return new HoverTip(title, description, (Texture2D?)null);
        };
    }

    // Read-only: hover tips are display-only, never mutate state here.
    // _owner is protected, so it can only be read through a PatchUpVar reference (clones keep the type).
    private static Creature? GetOwnerCreature(DynamicVar var) => (var as PatchUpVar)?._owner switch
    {
        CardModel card => card.Owner?.Creature,
        PowerModel power => power.Owner,
        _ => null,
    };
}
