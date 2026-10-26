using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.RestSiteOptions;

/// <summary>
/// Chaos Emerald sidequest, step 1: add a Special Stage Quest card to the deck. Once per run, for every character,
/// and only while the player doesn't hold their own Emerald.
/// </summary>
public class Seek(Player owner) : CustomRestSiteOption(owner)
{
    public override string OptionId => "SeekChaosEmerald";

    public override string? CustomIconPath => Path.Join(MainFile.ResPath, "images", "ui", "rest", "seek.png");

    public override async Task<bool> OnSelect()
    {
        var grantsCombo = await SyncAllEmeraldsUnlocked();

        var card = Owner.RunState.CreateCard<SpecialStage>(Owner);
        card.GrantsCombo = grantsCombo;
        ChaosEmeraldHelper.HasSought.Set(Owner, true);

        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        return true;
    }

    /// <summary>
    /// Unlocks live in each player's local save file, so the owner's client broadcasts whether all seven are unlocked
    /// and every other client waits for that answer (same mechanism as card choices).
    /// </summary>
    private async Task<bool> SyncAllEmeraldsUnlocked()
    {
        var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
        var choiceId = synchronizer.ReserveChoiceId(Owner);
        var choiceContext = new BlockingPlayerChoiceContext();
        await choiceContext.SignalPlayerChoiceBegun(PlayerChoiceOptions.None);

        bool allUnlocked;
        if (LocalContext.IsMe(Owner) && RunManager.Instance.NetService.Type != NetGameType.Replay)
        {
            allUnlocked = ChaosEmeraldHelper.AllUnlocked();
            synchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromIndex(allUnlocked ? 1 : 0));
        }
        else
        {
            allUnlocked = (await synchronizer.WaitForRemoteChoice(Owner, choiceId)).AsIndex() == 1;
        }

        await choiceContext.SignalPlayerChoiceEnded();
        return allUnlocked;
    }
}
