using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Base class for Shion's Team-Up cards: multiplayer Rares played by two players.
/// When the Initiator plays it, every Partner of the right class gets an Ethereal copy.
/// The effect happens when one Partner plays that copy (the other copies are Exhausted), and both players animate.
/// The copy shows the ".partnerDescription" text (TeamUpDescriptionPatch).
/// Team-Ups are never generated in combat or by modifiers, and loot only offers them with the Partner's class in the run (TeamUpLootPatch).
/// </summary>
public abstract class ShionTeamUpCard(int cost, CardType type, TargetType target) :
    ShionTheBruiserCard(cost, type, CardRarity.Rare, target)
{
    /// <summary>The Partner's class. Null when any class can be the Partner.</summary>
    public abstract CharacterModel? PartnerCharacter { get; }

    public virtual CharacterModel InitiatorCharacter => ModelDb.Character<Character.ShionTheBruiser>();

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public sealed override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.TeamUp, ..TeamUpKeywords];

    protected virtual IEnumerable<CardKeyword> TeamUpKeywords => [];

    /// <summary>The player who played the original card. Only set on the Partner's copy.</summary>
    public Player? Initiator { get; private set; }

    public bool IsPartnerCopy => Initiator != null;

    // The Initiator's play only hands out the copies, so it never needs a target.
    public override TargetType TargetType => IsPartnerCopy ? base.TargetType : TargetType.Self;

    // Shared by all the copies one play created, so playing one Exhausts the others.
    private List<ShionTeamUpCard> _copies = [];

    protected override bool IsPlayable => Initiator is { } initiator
        ? initiator.Creature.IsAlive
        : TeamUpHelper.GetPartners(this, Owner).Any();

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (Initiator is { } initiator)
        {
            await ExhaustOtherCopies(choiceContext);
            await PlayTeamUpAnimations(initiator.Creature, Owner.Creature);
            await OnTeamUp(choiceContext, play, initiator, Owner);
            return;
        }

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        List<ShionTeamUpCard> copies = [];
        foreach (var partner in TeamUpHelper.GetPartners(this, Owner).ToList())
        {
            var copy = (ShionTeamUpCard)SlalMechanicsHelper.CreateCopyForPlayer(this, partner);
            copy.Initiator = Owner;
            copy._copies = copies;
            copy.AddKeyword(CardKeyword.Ethereal);
            copies.Add(copy);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
        }
    }

    /// <summary>
    /// The Team-Up effect, played from the Partner's copy.
    /// </summary>
    protected abstract Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner);

    private async Task ExhaustOtherCopies(PlayerChoiceContext choiceContext)
    {
        foreach (var copy in _copies.Where(c => c != this && c.Pile is { IsCombatPile: true } && c.Pile.Type != PileType.Exhaust).ToList())
        {
            await CardCmd.Exhaust(choiceContext, copy);
        }
    }

    // Attacks already animate their attacker (the Partner) through DamageCmd.
    private async Task PlayTeamUpAnimations(Creature initiator, Creature partner)
    {
        switch (Type)
        {
            case CardType.Attack:
                await CreatureCmd.TriggerAnim(initiator, "Attack", 0f);
                break;
            case CardType.Power:
                await CreatureCmd.TriggerAnim(initiator, "PowerUp", 0f);
                await CreatureCmd.TriggerAnim(partner, "PowerUp", partner.Player!.Character.PowerUpAnimDelay);
                break;
            default:
                await CreatureCmd.TriggerAnim(initiator, "Cast", 0f);
                await CreatureCmd.TriggerAnim(partner, "Cast", partner.Player!.Character.CastAnimDelay);
                break;
        }
    }
}
