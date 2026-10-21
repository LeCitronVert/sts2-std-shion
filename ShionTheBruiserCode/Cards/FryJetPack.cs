using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Retain. Gain 16 (20) Block. Add 3 Burns into your Draw Pile.
/// Transform this card into Jettison Jetpack Attack and return it to your Hand.
/// </summary>
public class FryJetPack() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override int CanonicalStarCost => 1;

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(16M, ValueProp.Move),
        new CardsVar(3),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Burn>(),
        HoverTipFactory.FromCard<FryJettisonJetpackAttack>(IsUpgraded),
    ];

    // Set when played, so the card transforms once it is back in the Hand.
    private bool _transformInHand;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await CardPileCmd.AddToCombatAndPreview<Burn>(Owner.Creature, PileType.Draw, DynamicVars.Cards.IntValue, Owner, CardPilePosition.Random);
        _transformInHand = true;
    }

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        return card == this && pileType == PileType.Discard ? (PileType.Hand, position) : (pileType, position);
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card != this || !_transformInHand || Pile?.Type != PileType.Hand || CombatState is null) return;
        _transformInHand = false;

        var jettison = CombatState.CreateCard<FryJettisonJetpackAttack>(Owner);
        if (IsUpgraded) CardCmd.Upgrade(jettison);
        await CardCmd.Transform(this, jettison);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4M);
    }
}
