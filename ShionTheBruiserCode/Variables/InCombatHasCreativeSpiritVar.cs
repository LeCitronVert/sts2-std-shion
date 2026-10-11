using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Variables;

/// <summary>
/// DynamicVar that tracks whether CreativeSpirit power is active for Fresh Idea cards.
/// Returns 1 if active, 0 otherwise.
/// </summary>
public class InCombatHasCreativeSpiritVar : DynamicVar
{
    public const string Key = "InCombatHasCreativeSpirit";
    private readonly CardModel? _card;

    public InCombatHasCreativeSpiritVar(CardModel? card = null) : base(Key, 0)
    {
        _card = card;
    }

    protected override decimal GetBaseValueForIConvertible()
    {
        if (_card is null || _card.Owner is null || _card.Owner.Creature is null)
            return 0;

        return _card.Owner.Creature.Powers.Any(p => p is CreativeSpirit) ? 1 : 0;
    }
}




