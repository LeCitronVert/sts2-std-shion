using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

[Pool(typeof(ShionTheBruiserRelicPool))]
public class EdgeTheChao() : ShionTheBruiserRelic
{
    // Shion relic
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public const int VigorNextTurnAmount = 5;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VigorNextTurn>(VigorNextTurnAmount)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<VigorNextTurn>()
    ];

    // Galvanised Attack = an Attack whose own Galvanised effect triggered during this play
    // (tracked by ShionMechanicsHelper.TriggerGalvanised(CardModel, ...), shared with Star Rod).
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner != Owner || card.Type != CardType.Attack) return Task.CompletedTask;

        // Each play (replays included) starts fresh
        ShionMechanicsHelper.ClearCardGalvanised(card);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner != Owner || card.Type != CardType.Attack || !ShionMechanicsHelper.WasCardGalvanised(card))
            return;

        Flash();
        CompanionVisuals.PlayEdgeProc(Owner.Creature);
        await PowerCmd.Apply<VigorNextTurn>(choiceContext, Owner.Creature, DynamicVars["VigorNextTurn"].BaseValue, Owner.Creature, null);
    }
}