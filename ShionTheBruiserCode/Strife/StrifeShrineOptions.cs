using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// The Chaos Emeralds shrine's three choices (<see cref="StrifeShrine"/>), as relics like other Ancients' options.
/// Names are placeholders from the request ("Placeholder A/B/C").
/// </summary>
[Pool(typeof(EventRelicPool))]
public abstract class StrifeShrineOption : ShionTheBruiserRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    // No outline drawn yet (MISSING_IMAGES.tsv)
    protected override string PackedIconOutlinePath => "relic_outline.png".RelicImagePath();
}

/// <summary>Placeholder A: gain 50 Rings at the start of the next fight.</summary>
public class StrifeShrineRings : StrifeShrineOption
{
    public override string PackedIconPath => RingVisuals.RingIconPath;
    protected override string BigIconPath => RingVisuals.RingIconPath;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Rings", 50M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Rings>()];

    public override bool IsUsedUp => Used;

    private bool _used;

    [SavedProperty]
    public bool Used
    {
        get => _used;
        set
        {
            AssertMutable();
            _used = value;
            if (_used) Status = RelicStatus.Disabled;
        }
    }

    public override async Task BeforeCombatStart()
    {
        if (Used) return;
        Used = true;
        Flash();
        await Rings.Gain(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars["Rings"].BaseValue);
    }
}

/// <summary>Placeholder B: upon pickup, choose 1 of 3 upgraded Rare cards of your character (can be skipped, like a card reward).</summary>
public class StrifeShrineCard : StrifeShrineOption
{
    public const int Choices = 3;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        var pool = new CardCreationOptions([Owner.Character.CardPool], CardCreationSource.Other, CardRarityOddsType.Uniform,
                c => c.Rarity == CardRarity.Rare)
            .GetPossibleCards(Owner)
            .OrderBy(c => c.Id)
            .ToList();

        var rng = Owner.PlayerRng.Rewards;
        List<CardModel> choices = [];
        while (choices.Count < Choices && pool.Count > 0)
        {
            var card = rng.NextItem(pool)!;
            pool.Remove(card);
            var choice = Owner.RunState.CreateCard(card, Owner);
            if (choice.IsUpgradable) CardCmd.Upgrade(choice);
            choices.Add(choice);
        }
        if (choices.Count == 0) return;

        var chosen = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), choices, Owner, canSkip: true);
        if (chosen != null) CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosen, PileType.Deck));
    }
}

/// <summary>Placeholder C: upon pickup, obtain a random Rare relic and fill your empty potion slots with random Rare potions.</summary>
public class StrifeShrineRelic : StrifeShrineOption
{
    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(Owner, RelicRarity.Rare).ToMutable(), Owner);

        var rarePotions = PotionFactory.GetPotionOptions(Owner).Where(p => p.Rarity == PotionRarity.Rare).OrderBy(p => p.Id).ToList();
        if (rarePotions.Count == 0) return;
        var rng = Owner.PlayerRng.Rewards;
        var open = Owner.PotionSlots.Count(p => p == null);
        for (var i = 0; i < open && Owner.HasOpenPotionSlots; i++)
        {
            await PotionCmd.TryToProcure(rng.NextItem(rarePotions)!.ToMutable(), Owner);
        }
    }
}
