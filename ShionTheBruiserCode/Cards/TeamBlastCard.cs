using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Base class for the Team Blasts of the vanilla characters and the generic (Colorless) ones.
/// Each subclass puts itself in its pool with [Pool]. See <see cref="Singletons.TeamBlastSingleton"/> for how Team Blasts work.
/// </summary>
public abstract class TeamBlastCard(CardType type, TargetType target) :
    CustomCardModel(0, type, CardRarity.Rare, target)
{
    // ── Image paths ──────────────────────────────────────────────────────────
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public sealed override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.TeamBlast, ..TeamBlastKeywords];

    protected virtual IEnumerable<CardKeyword> TeamBlastKeywords => [];

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await OnTeamBlast(choiceContext, play);
        await DreamersMechanicsHelper.ExhaustAllTeamBlasts(choiceContext, play);
    }

    protected abstract Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play);

    /// <summary>Living players on this card's owner's team.</summary>
    protected IEnumerable<MegaCrit.Sts2.Core.Entities.Players.Player> LivingPlayers =>
        CombatState?.GetTeammatesOf(Owner.Creature).Where(c => c is { IsPlayer: true, IsAlive: true }).Select(c => c.Player!).ToList() ?? [];
}
