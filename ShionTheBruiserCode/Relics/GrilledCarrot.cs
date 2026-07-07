using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Patches;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.RestSite;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class GrilledCarrot : ShionTheBruiserRelic, IRestSiteActionRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    private const int HealAmount = 5;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(HealAmount)
    ];
    
    public async Task OnRestSiteActionTaken(Player player, RestSiteOption restSiteOption)
    {
        // Add null check for Owner
        if (Owner == null || player != this.Owner || restSiteOption.OptionId == "HEAL")
            return;

        Flash();
        await CreatureCmd.Heal(player.Creature, HealAmount);
    }

}