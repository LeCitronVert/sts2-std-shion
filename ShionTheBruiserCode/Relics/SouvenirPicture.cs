using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// If you end your turn without using a Berserk effect, gain 2 Vigor at the start of your next turn.
/// </summary>
public class SouvenirPicture() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Common;

    public const int VigorAmount = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("VigorAmount", VigorAmount)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner.Creature)) return;
        if (ShionMechanicsHelper.BerserkUsedThisTurn(Owner.Creature)) return;

        Flash();
        await PowerCmd.Apply<VigorNextTurn>(choiceContext, Owner.Creature, VigorAmount, Owner.Creature, null);
    }
}
