using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Necrobinder Team Blast. This turn (and next turn), the targeted enemy gains Doom equal to the attack damage it receives.
/// </summary>
[Pool(typeof(NecrobinderCardPool))]
public class NecrobinderLegionOfDeath() : TeamBlastCard(CardType.Skill, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Turns", 1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<LegionOfDeath>(),
        HoverTipFactory.FromPower<DoomPower>(),
    ];

    protected override async Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<LegionOfDeath>(choiceContext, play.Target, DynamicVars["Turns"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Turns"].UpgradeValueBy(1);
    }
}
