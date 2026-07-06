using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;

namespace ShionTheBruiser.ShionTheBruiserCode.Patches;

public interface IRestSiteActionRelic
{
    Task OnRestSiteActionTaken(Player player, RestSiteOption restSiteOption);
}