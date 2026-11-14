using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Base class for Seels The Reckless's cards.
/// Sets up image paths.
/// </summary>
[Pool(typeof(SeelsTheRecklessCardPool))]
public abstract class SeelsTheRecklessCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    // ── Image paths ──────────────────────────────────────────────────────────
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    // ── Animation ────────────────────────────────────────────────────────────
    /// <summary>
    /// Animation Seels plays when he plays this card (Singletons/SeelsAnimationSingleton.cs): "Cast" for Skills and
    /// Powers, none for Attacks (their damage plays "Attack"). Blood Bend Skills return null: Blood Bend plays "Blood".
    /// </summary>
    public virtual string? PlayAnimation => Type is CardType.Skill or CardType.Power ? "Cast" : null;
}

