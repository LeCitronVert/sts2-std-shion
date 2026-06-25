using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

public class CritHitRateVar(decimal rateValue): DynamicVar(Key, rateValue)
{
    public const string Key = "CriticalHitRate";
}