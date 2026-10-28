using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;

namespace ShionTheBruiser.ShionTheBruiserCode.Timeline;

/// <summary>
/// Removes the content of unrevealed epochs from pools, like the base pools do in FilterThroughEpochs.
/// Every pool of the mod calls this, so locking content only means listing it on an epoch.
/// </summary>
public static class EpochLocks
{
    private static List<(string EpochId, HashSet<ModelId> Content)>? _locks;

    // Built on first use: the epochs' content lists need ModelDb
    private static List<(string EpochId, HashSet<ModelId> Content)> Locks => _locks ??= DreamersEpoch.All
        .Select(e => (e.Id, e.Cards.Select(c => c.Id).Concat(e.Relics.Select(r => r.Id)).Concat(e.Potions.Select(p => p.Id)).ToHashSet()))
        .Where(l => l.Item2.Count > 0)
        .ToList();

    public static IEnumerable<T> Filter<T>(UnlockState unlockState, IEnumerable<T> models) where T : AbstractModel
    {
        var locked = Locks
            .Where(l => !EpochProgress.IsRevealed(unlockState, l.EpochId))
            .SelectMany(l => l.Content)
            .ToHashSet();
        return locked.Count == 0 ? models : models.Where(m => !locked.Contains(m.Id)).ToList();
    }
}
