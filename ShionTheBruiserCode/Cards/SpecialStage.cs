using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Quest card added by the Seek rest site option.
/// At the start of the next Elite fight, its combat copy is Exhausted and the team gets the owner's Emerald challenge.
/// If the team wins, the deck card is removed and the owner gains their Chaos Emerald (or Chaos Emeralds).
/// The deck card stays in the deck during that fight so that reloading the fight triggers it again.
/// </summary>
[Pool(typeof(QuestCardPool))]
public class SpecialStage() : ShionTheBruiserCard(-1,
    CardType.Quest, CardRarity.Quest,
    TargetType.Self)
{
    public override int MaxUpgradeLevel => 0;
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    private bool _grantsCombo;

    /// <summary>
    /// Whether the owner had all seven Emeralds unlocked when they Sought (synced from the owner's save file).
    /// </summary>
    [SavedProperty]
    public bool GrantsCombo
    {
        get => _grantsCombo;
        set
        {
            AssertMutable();
            _grantsCombo = value;
        }
    }

    // Set when the quest triggers at the start of an Elite fight; not saved (a reload restarts the fight).
    private bool _triggered;

    private Player? OwnerOrNull => IsMutable ? Owner : null;

    private ChaosEmeraldColor? EmeraldColor => OwnerOrNull is { } owner ? ChaosEmeraldHelper.ColorFor(owner) : null;

    private bool IsInDeck => Pile?.Type == PileType.Deck;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (EmeraldColor is not { } color) return [];
            var relic = GrantsCombo ? ChaosEmeraldHelper.ComboRelic(color) : ChaosEmeraldHelper.SoloRelic(color);
            return ChallengeHoverTips(color).Concat(HoverTipFactory.FromRelic(relic));
        }
    }

    private static IEnumerable<IHoverTip> ChallengeHoverTips(ChaosEmeraldColor color) => color switch
    {
        ChaosEmeraldColor.Purple => [HoverTipFactory.FromPower<PurpleEmeraldChallenge>()],
        ChaosEmeraldColor.Cyan => [HoverTipFactory.FromPower<CyanEmeraldChallenge>()],
        ChaosEmeraldColor.Green => [HoverTipFactory.FromPower<GreenEmeraldChallenge>()],
        ChaosEmeraldColor.Yellow => [HoverTipFactory.FromPower<YellowEmeraldChallenge>()],
        ChaosEmeraldColor.Blue => [HoverTipFactory.FromPower<BlueEmeraldChallenge>()],
        ChaosEmeraldColor.Red => [HoverTipFactory.FromPower<RitualPower>()],
        _ => [HoverTipFactory.FromPower<GreyEmeraldChallenge>()],
    };

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        var colorKey = EmeraldColor is { } color ? ChaosEmeraldHelper.ColorKey(color) : "any";
        description.Add("Challenge", new LocString("cards", $"SHIONTHEBRUISER-SPECIAL_STAGE.challenge_{colorKey}"));
        description.Add("Emerald", EmeraldColor is { } c
            ? (GrantsCombo ? ChaosEmeraldHelper.ComboRelic(c) : ChaosEmeraldHelper.SoloRelic(c)).Title
            : new LocString("cards", "SHIONTHEBRUISER-SPECIAL_STAGE.emerald_any"));
    }

    public override async Task BeforeCombatStart()
    {
        _triggered = false;
        if (!IsInDeck || EmeraldColor is not { } color) return;
        var combatState = Owner.Creature.CombatState;
        if (combatState?.Encounter?.RoomType != RoomType.Elite) return;

        _triggered = true;
        var choiceContext = new BlockingPlayerChoiceContext();

        // The quest must leave the draw pile: exhaust this fight's copy of the card
        foreach (var copy in Owner.PlayerCombatState?.AllCards.Where(c => c.DeckVersion == this).ToList() ?? [])
        {
            await CardCmd.Exhaust(choiceContext, copy);
        }

        await ChaosEmeraldHelper.ApplyChallenge(color, combatState, Owner, choiceContext);
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (!_triggered || !IsInDeck || EmeraldColor is not { } color) return;
        _triggered = false;

        var owner = Owner;
        var relic = GrantsCombo ? ChaosEmeraldHelper.ComboRelic(color) : ChaosEmeraldHelper.SoloRelic(color);
        await CardPileCmd.RemoveFromDeck(this);
        await RelicCmd.Obtain(relic.ToMutable(), owner);
    }
}
