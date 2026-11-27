using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Strife;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Base class for the Chaos Emerald relics, only obtainable through the Special Stage quest.
/// EventRelicPool never feeds random relic drops, so these can't be found anywhere else.
/// At the start of each combat, applies its passive (<see cref="EmeraldPassive"/>) and adds its Emerald Power card (its
/// Burst, which turns the passive off from the next turn on) to the Draw Pile. With <see cref="SuperForm"/>, the card
/// isn't added: it's picked from the Emerald wheel into the Hand instead (<see cref="EmeraldWheel"/>).
/// The combo ("Chaos Emeralds") variants share the effect of their solo version and unlock Act 4.
/// </summary>
[Pool(typeof(EventRelicPool))]
public abstract class ChaosEmeraldRelic : ShionTheBruiserRelic
{
    public abstract ChaosEmeraldColor Color { get; }

    public virtual bool IsCombo => false;

    public override RelicRarity Rarity => RelicRarity.Event;

    private string Variant => IsCombo ? "combo" : "solo";

    public override string PackedIconPath =>
        $"emeralds/emerald_{Variant}_{ChaosEmeraldHelper.ColorKey(Color)}.png".RelicImagePath();

    protected override string PackedIconOutlinePath => $"emeralds/emerald_{Variant}_white.png".RelicImagePath();

    protected override string BigIconPath => PackedIconPath;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard(ChaosEmeraldHelper.EmeraldPowerCard(Color)),
        HoverTipFactory.FromKeyword(DreamersKeywords.EmeraldPower),
    ];

    public override async Task BeforeCombatStart()
    {
        await EmeraldPassive.Apply(new ThrowingPlayerChoiceContext(), Owner.Creature, Color);

        // Super Form: picked from the Emerald wheel instead
        if (SuperForm.Has(Owner)) return;

        var card = Owner.Creature.CombatState?.CreateCard(ChaosEmeraldHelper.EmeraldPowerCard(Color), Owner);
        if (card is null) return;

        CardCmd.PreviewCardPileAdd(
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, Owner, CardPilePosition.Random)
        );
    }
}
