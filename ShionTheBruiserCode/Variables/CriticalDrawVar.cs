using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

public class CriticalDrawVar: DynamicVar
{
    public const string Key = "CriticalDraw";
    
    public CriticalDrawVar(decimal criticalDrawValue) : base(Key, criticalDrawValue)
    {
        this.WithTooltip();
    }
}