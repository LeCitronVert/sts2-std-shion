using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

public interface ICardAfflictionListener
{
    public Task OnAfflictionApplied(AfflictionModel affliction, CardModel card);
}
