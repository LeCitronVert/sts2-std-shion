using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

public class CritAttackRateVar(decimal rateValue) : DynamicVar(Key, rateValue)
{
    public const string Key = "CriticalAttackRate";
}