using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Grants 7 Strength and 7 Dexterity, removed after X turns.
/// </summary>
public class MechaExpansionPack : FryTheCommanderPower
{
    public const int StatBonus = 7;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Stats", StatBonus)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<StrengthPower>(context, Owner, StatBonus, Owner, null);
        await PowerCmd.Apply<DexterityPower>(context, Owner, StatBonus, Owner, null);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -StatBonus, Owner, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, -StatBonus, Owner, null);
        await PowerCmd.Remove(this);
    }
}
