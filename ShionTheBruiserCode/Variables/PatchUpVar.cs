using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

public class PatchUpVar: DynamicVar
{
    public const string Key = "PatchUp";
    
    public PatchUpVar(decimal patchUpValue) : base(Key, patchUpValue)
    {
        this.WithTooltip();
    }
}