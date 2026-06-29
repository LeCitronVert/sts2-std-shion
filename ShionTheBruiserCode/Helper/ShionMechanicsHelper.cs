using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

public static class ShionMechanicsHelper
{
    public static int GetDragonUniqueBuffCount(Creature creature)
    {
        // Get count of unique Buffs.
        return creature.Powers.Where(p => p.Type == PowerType.Buff).DistinctBy(p => p.GetType()).Count();
    }
    
    public static bool HasVigor(Creature creature) 
    {
        return creature.HasPower<VigorPower>();
    }
    
    public static int GetStrengthAmount(Creature creature)
    {
        return creature.GetPower<StrengthPower>()?.Amount ?? 0;
    }
}