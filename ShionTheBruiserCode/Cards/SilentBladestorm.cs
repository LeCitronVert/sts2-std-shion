using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Silent Team Blast. ALL players fill their Hand with Shivs (Shivs+).
/// </summary>
[Pool(typeof(SilentCardPool))]
public class SilentBladestorm() : TeamBlastCard(CardType.Skill, TargetType.AllAllies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Shiv>(IsUpgraded)];

    protected override async Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;

        foreach (var player in LivingPlayers)
        {
            // This card is still in the Hand while it resolves, so only count the other cards
            var room = CardPile.MaxCardsInHand - PileType.Hand.GetPile(player).Cards.Count(c => c != this);
            if (room <= 0) continue;

            var shivs = await Shiv.CreateInHand(player, room, CombatState);
            if (!IsUpgraded) continue;
            foreach (var shiv in shivs)
            {
                CardCmd.Upgrade(shiv);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
