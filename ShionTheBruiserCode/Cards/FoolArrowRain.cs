#if STS2_BETA
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Fool Team Blast. Add an Aim Assist (upgraded: Aim Assist+) into ALL other players' Hands. Upgraded: they also gain 1 Energy.
/// Aim Assist gives Aim to the player who used this card. Beta only (the Fool doesn't exist on the main branch).
/// </summary>
[Pool(typeof(FoolCardPool))]
public class FoolArrowRain() : TeamBlastCard(CardType.Skill, TargetType.AllAllies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<FoolAimAssist>(IsUpgraded)];

    protected override async Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;

        foreach (var player in LivingPlayers.Where(p => p != Owner))
        {
            var card = CombatState.CreateCard<FoolAimAssist>(player);
            card.SetShooter(Owner);
            if (IsUpgraded) CardCmd.Upgrade(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);

            if (IsUpgraded)
            {
                await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
#endif
