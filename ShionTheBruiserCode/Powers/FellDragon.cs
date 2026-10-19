using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

// Always apply Galvanised and Berserk effects (see ShionMechanicsHelper.IsGalvanised / IsBerserk),
// Patch Up limit reduced (see ShionMechanicsHelper.GetPatchUpThreshold)
public class FellDragon : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

    public const int HpLoss = 5;
    public const int PatchUpReduction = 20;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HpLossVar(HpLoss),
        new DynamicVar("PatchUpReduction", PatchUpReduction),
        new PatchUpVar(0)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        
        Flash();
        await CreatureCmd.Damage(choiceContext, Owner, HpLoss, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
    }
}
