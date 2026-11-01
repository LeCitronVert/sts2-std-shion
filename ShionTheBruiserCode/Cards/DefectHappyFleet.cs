using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Defect Team Blast. ALL other players (upgraded: ALL players) gain as many Orb Slots as you have,
/// and Channel a copy of each of your Orbs.
/// </summary>
[Pool(typeof(DefectCardPool))]
public class DefectHappyFleet() : TeamBlastCard(CardType.Skill, TargetType.AllAllies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Channeling)];

    protected override async Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (Owner.PlayerCombatState is not { } ownerState) return;

        // Snapshot first, since the Defect may receive the copies too
        var slots = ownerState.OrbQueue.Capacity;
        var orbs = ownerState.OrbQueue.Orbs.Select(o => o.Id).ToList();

        foreach (var player in LivingPlayers.Where(p => IsUpgraded || p != Owner))
        {
            if (slots > 0)
            {
                await OrbCmd.AddSlots(player, slots);
            }
            foreach (var orbId in orbs)
            {
                await OrbCmd.Channel(choiceContext, ModelDb.GetById<OrbModel>(orbId).ToMutable(), player);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
