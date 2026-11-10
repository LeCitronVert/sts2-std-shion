using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Cosmo (cursed). Upon pickup, Transform 3 of your basic Strikes into an Attack, a Skill and a Power.
/// Whenever you play an Attack, lose 2 Strength this turn.
/// </summary>
public class PeacePromise : CosmoRelic
{
    public override CosmoAvailability CosmoAvailability => CosmoAvailability.Always;

    public override bool IsCursedOption => true;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new PowerVar<StrengthPower>(2m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public override async Task AfterObtained()
    {
        var strikes = PileType.Deck.GetPile(Owner).Cards
            .Where(c => c.Rarity == CardRarity.Basic && c.Tags.Contains(CardTag.Strike) && c.IsRemovable)
            .Take(DynamicVars.Cards.IntValue)
            .ToList();
        if (strikes.Count == 0) return;

        // One of each type, in this order (with fewer Strikes, the first types are used).
        CardType[] types = [CardType.Attack, CardType.Skill, CardType.Power];
        var transformations = strikes.Select((c, i) => new CardTransformation(c, CreateTransformOfType(c, types[i % types.Length])));
        var results = (await CardCmd.Transform(transformations, null, CardPreviewStyle.None)).ToList();
        if (results.Count > 0 && LocalContext.IsMe(Owner))
        {
            NSimpleCardsViewScreen.ShowScreen(results, new LocString("relics", "SHIONTHEBRUISER-PEACE_PROMISE.infoText"));
        }
    }

    private CardModel CreateTransformOfType(CardModel original, CardType type)
    {
        var options = CardFactory.GetDefaultTransformationOptions(original, isInCombat: false).ToList();
        // Same rarities the transform keeps, so the fallback check matches what it can actually pick.
        var ofType = options.Where(c => c.Type == type && c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare).ToList();
        // Falls back to any card if the pool has none of that type.
        return CardFactory.CreateRandomCardForTransform(original, ofType.Count > 0 ? ofType : options, isInCombat: false, Owner.RunState.Rng.Niche);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Attack) return;

        Flash();
        await PowerCmd.Apply<PeacePromiseStrengthDown>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, null);
    }
}
