using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Cooldown X: this card has Retain. When played, it is returned to your Hand and becomes free to play for X turns.
/// If played during those X turns, it is discarded with no additional effect.
/// </summary>
public abstract class FryCooldownCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    FryTheCommanderCard(cost, type, rarity, target)
{
    public const string CooldownKey = "Cooldown";
    public const string CooldownLeftKey = "CooldownLeft";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, FryCardKeywords.Cooldown, ..ExtraKeywords];

    protected virtual IEnumerable<CardKeyword> ExtraKeywords => [];

    protected abstract int BaseCooldown { get; }

    protected abstract IEnumerable<DynamicVar> CooldownCardVars { get; }

    protected sealed override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar(CooldownKey, BaseCooldown),
        new DynamicVar(CooldownLeftKey, 0),
        ..CooldownCardVars,
    ];

    public bool IsOnCooldown => CooldownLeft > 0;

    private int CooldownLeft
    {
        get => DynamicVars[CooldownLeftKey].IntValue;
        set => DynamicVars[CooldownLeftKey].BaseValue = value;
    }

    protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (IsOnCooldown)
        {
            CooldownLeft = 0;
            return;
        }

        await OnCooldownCardPlay(choiceContext, play);
        CooldownLeft = DynamicVars[CooldownKey].IntValue;
    }

    protected abstract Task OnCooldownCardPlay(PlayerChoiceContext choiceContext, CardPlay play);

    // The result pile is decided before OnPlay: played on cooldown, the card is discarded, otherwise it returns to the Hand.
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        if (card != this || pileType != PileType.Discard) return (pileType, position);
        return (IsOnCooldown ? PileType.Discard : PileType.Hand, position);
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || !IsOnCooldown) return false;
        modifiedCost = 0;
        return true;
    }

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || !IsOnCooldown) return false;
        modifiedCost = 0;
        return true;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner && IsOnCooldown) CooldownLeft--;
        return Task.CompletedTask;
    }
}
