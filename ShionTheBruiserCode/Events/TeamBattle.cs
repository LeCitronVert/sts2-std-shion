using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Events;

/// <summary>
/// Team Battle. Multiplayer only, Hive or Glory. Shared: the players vote, the winning option runs for everyone.
/// Fight them as a team: a Team Blast card reward (own pool + Colorless), add Clumsy.
/// Fight your counterpart one on one: a Rare card reward, add Injury.
/// Fight your teammate counterpart (only when the team has more than one class): two card rewards
/// from the teammates' pools, never from the player's own pool. Team-Ups the player could not play (no other
/// player of their Partner's class) are left out.
/// </summary>
public sealed class TeamBattle : DreamersEventModel
{
    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState) =>
        runState.Players.Count > 1 && (IsInAct<Hive>(runState) || IsInAct<Glory>(runState));

    private static bool HasSeveralClasses(IRunState runState) =>
        runState.Players.Select(p => p.Character.Id).Distinct().Count() > 1;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options =
        [
            new(this, AsTeam, $"{Id.Entry}.pages.INITIAL.options.AS_TEAM",
                [HoverTipFactory.FromKeyword(DreamersKeywords.TeamBlast), ..HoverTipFactory.FromCardWithCardHoverTips<Clumsy>()]),
            new(this, OneOnOne, $"{Id.Entry}.pages.INITIAL.options.ONE_ON_ONE", HoverTipFactory.FromCardWithCardHoverTips<Injury>()),
        ];
        // Same answer on every client (the run's players), so the vote sees the same options everywhere
        if (Owner != null && HasSeveralClasses(Owner.RunState))
        {
            options.Add(new EventOption(this, Teammate, $"{Id.Entry}.pages.INITIAL.options.TEAMMATE"));
        }
        return options;
    }

    private async Task AsTeam()
    {
        var player = Owner!;
        var options = new CardCreationOptions(
                [player.Character.CardPool, ModelDb.CardPool<ColorlessCardPool>()],
                CardCreationSource.Other, CardRarityOddsType.Uniform,
                c => c.CanonicalKeywords.Contains(DreamersKeywords.TeamBlast))
            .WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications);
        await RewardsCmd.OfferCustom(player, [new CardReward(options, 3, player)]);
        await CardPileCmd.AddCurseToDeck<Clumsy>(player);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.AS_TEAM.description"));
    }

    private async Task OneOnOne()
    {
        var player = Owner!;
        var options = new CardCreationOptions(
                [player.Character.CardPool], CardCreationSource.Other, CardRarityOddsType.Uniform,
                c => c.Rarity == CardRarity.Rare)
            .WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications);
        await RewardsCmd.OfferCustom(player, [new CardReward(options, 3, player)]);
        await CardPileCmd.AddCurseToDeck<Injury>(player);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.ONE_ON_ONE.description"));
    }

    private async Task Teammate()
    {
        var player = Owner!;
        // Every teammate class except the player's own; several teammates mix their pools
        var pools = player.RunState.Players
            .Where(p => p != player && p.Character.Id != player.Character.Id)
            .Select(p => p.Character.CardPool)
            .DistinctBy(p => p.Id)
            .ToList();
        if (pools.Count > 0)
        {
            List<Reward> rewards = [];
            for (var i = 0; i < 2; i++)
            {
                rewards.Add(new CardReward(CardCreationOptions.ForNonCombatWithDefaultOdds(pools,
                    c => TeamUpHelper.IsPlayableByOwner(c, player)), 3, player));
            }
            await RewardsCmd.OfferCustom(player, rewards);
        }
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.TEAMMATE.description"));
    }
}
