using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Made of Explodium (Team-Up): the next Amount time(s) you lose HP, gain Flashback equal to the amount lost, with an explosion.
/// </summary>
public class MadeOfExplodium : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Flashback>()];

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner || delta >= 0 || Owner.IsDead) return;

        Flash();
        // The same explosion as Inferno's
        NCombatRoom.Instance?.CombatVfxContainer?.AddChildSafely(NFireBurstVfx.Create(Owner, 0.75f));
        await PowerCmd.Apply<Flashback>(new ThrowingPlayerChoiceContext(), Owner, -delta, Owner, null);
        await PowerCmd.Decrement(this);
    }
}
