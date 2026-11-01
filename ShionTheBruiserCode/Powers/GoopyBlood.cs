using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Goopy Blood (Team-Up), on Seels: whenever your Partner creates a Slimed, gain 1 Strength (times Amount).
/// Whenever you create a Wound, your Partner gains 2 Random (times Amount).
/// </summary>
public class GoopyBlood : TeamUpPower
{
    public const int StrengthPerSlimed = 1;
    public const int RandomPerWound = 2;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Strength", StrengthPerSlimed),
        new DynamicVar("Random", RandomPerWound),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromCard<Wound>(),
        HoverTipFactory.FromPower<Random>(),
    ];

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (creator == null || Owner.IsDead) return;

        if (card is Slimed && Partners.Contains(creator.Creature))
        {
            Flash();
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, StrengthPerSlimed * Amount, Owner, null);
        }
        else if (card is Wound && creator == Owner.Player)
        {
            foreach (var partner in LivingPartners.ToList())
            {
                Flash();
                await SlalMechanicsHelper.GainRandom(new ThrowingPlayerChoiceContext(), partner, RandomPerWound * Amount);
            }
        }
    }
}
