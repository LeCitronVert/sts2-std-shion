using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo. Whenever you Smith at a Rest Site, also Upgrade a random card in your Deck.
/// Name from the sheet as is: its note says the full name is still to be checked.
/// </summary>
public class SuperBulldozuratorPuzzlitowersonic : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    public override Task AfterRestSiteSmith(Player player)
    {
        if (player != Owner) return Task.CompletedTask;

        var upgradable = PileType.Deck.GetPile(Owner).Cards.Where(c => c.IsUpgradable).ToList();
        if (upgradable.Count == 0) return Task.CompletedTask;

        Flash();
        CardCmd.Upgrade(Owner.RunState.Rng.Niche.NextItem(upgradable)!, CardPreviewStyle.HorizontalLayout);
        return Task.CompletedTask;
    }
}
