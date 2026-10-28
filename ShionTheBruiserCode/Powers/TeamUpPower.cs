using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Base class for the powers granted by Team-Up cards, which remember the Partners they work with.
/// </summary>
public abstract class TeamUpPower : ShionTheBruiserPower
{
    private sealed class Data
    {
        public readonly List<Creature> Partners = [];
    }

    protected override object InitInternalData() => new Data();

    public IReadOnlyList<Creature> Partners => GetInternalData<Data>().Partners;

    protected IEnumerable<Creature> LivingPartners => Partners.Where(p => p.IsAlive);

    /// <summary>
    /// Applies the power to the owner, linked to the given Partner.
    /// </summary>
    public static async Task Apply<T>(PlayerChoiceContext choiceContext, Creature owner, Creature partner, decimal amount, CardModel card)
        where T : TeamUpPower
    {
        await PowerCmd.Apply<T>(choiceContext, owner, amount, owner, card);

        var partners = owner.GetPower<T>()?.GetInternalData<Data>().Partners;
        if (partners != null && !partners.Contains(partner))
        {
            partners.Add(partner);
        }
    }
}
