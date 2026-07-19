using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

public interface ICardAfflictionListener
{
    public void OnAfflictionApplied(AfflictionModel affliction, CardModel card);
}