using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

public class DamageCapVar: DynamicVar
{
    public const string Key = "DamageCap";
    
    public DamageCapVar(decimal damageCapValue) : base(Key, damageCapValue)
    {
        this.WithTooltip();
    }
}