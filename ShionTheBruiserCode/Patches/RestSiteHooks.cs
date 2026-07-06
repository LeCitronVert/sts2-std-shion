using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

public static class RestSiteHooks
{
    public static async Task OnRestSiteActionTaken(Player player, RestSiteOption restSiteOption)
    {
        // Iterate through all relics the player owns
        foreach (RelicModel relic in player.Relics)
        {
            // Check if the relic implements our custom interface
            if (relic is IRestSiteActionRelic restSiteRelic)
            {
                await restSiteRelic.OnRestSiteActionTaken(player, restSiteOption);
            }
        }
    }
}