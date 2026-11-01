using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Generic Team Blast. ALL players gain 10 (20) Block and heal 10 HP.
/// </summary>
[Pool(typeof(ColorlessCardPool))]
public class GoodPals() : TeamBlastCard(CardType.Skill, TargetType.AllAllies)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10, ValueProp.Move),
        new HealVar(10),
    ];

    protected override async Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (var player in LivingPlayers)
        {
            await CreatureCmd.GainBlock(player.Creature, DynamicVars.Block, play);
            await CreatureCmd.Heal(player.Creature, DynamicVars.Heal.BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(10);
    }
}
