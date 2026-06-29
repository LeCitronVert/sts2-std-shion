using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

// todo : i can probably do it better than this but idk how rn
public class CritDisplayTooltipVar : DynamicVar
{
    public const string Key = "CriticalHit";
    
    // todo : change that
    public CritDisplayTooltipVar() : base(Key, 0)
    {
        this.WithTooltip();
    }
}


