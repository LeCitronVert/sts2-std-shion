using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Seels' curse, only obtained from The Gambler event.
/// Curse. Innate. Sly. If this card is transformed or exhausted, gain 150 gold. Exhaust. Retain. Eternal.
/// Unplayable like every curse: Sly still auto-plays it when discarded, which Exhausts it (gold).
/// The transform half is in Patches/GreatFortunesTransformPatch.cs (Eternal only allows it in combat).
/// In the curse pool for the curse frame, but never rolled by random curse effects.
/// </summary>
[Pool(typeof(CurseCardPool))]
public class SeelsGreatFortunes() : CustomCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public const int GoldAmount = 150;

    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedByModifiers => false;
    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Unplayable,
        CardKeyword.Innate,
        CardKeyword.Sly,
        CardKeyword.Exhaust,
        CardKeyword.Retain,
        CardKeyword.Eternal,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(GoldAmount)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Transform)];

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card != this) return;
        await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);
    }
}
