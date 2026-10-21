namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Upgraded Live With The Pain: counts ALL your Wounds, including the Exhausted ones.
/// </summary>
public class LiveWithThePainPlus : LiveWithThePain
{
    protected override bool CountsExhaustedWounds => true;
}
