using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Patches;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Add Novel to a non-Novel card in your Hand (and upgrade it). If you do, gain 1 energy. Exhaust.
/// The card gets "Neo" in front of its title.
/// </summary>
public class SlalThinkingBoard() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.ForEnergy(this),
    ];

    private static bool CanGainNovel(CardModel card) => !card.Keywords.Contains(SlalCardKeywords.Novel);

    protected override bool ShouldGlowRedInternal => CombatState != null && !PreviewHelper.OtherCardsInHand(this).Any(CanGainNovel);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var chosen = (await CardSelectCmd.FromHand(
            choiceContext,
            card.Owner,
            new CardSelectorPrefs(card.SelectionScreenPrompt, 1),
            CanGainNovel,
            card
        )).FirstOrDefault();
        if (chosen is null) return;

        await SlalMechanicsHelper.EnsureCreatureHasNovelTracker(card.Owner.Creature);
        chosen.AddKeyword(SlalCardKeywords.Novel);
        NeoTitlePatch.MarkAsNeo(chosen);
        if (IsUpgraded && chosen.IsUpgradable)
        {
            CardCmd.Upgrade(chosen);
        }

        await PlayerCmd.GainEnergy(card.DynamicVars.Energy.BaseValue, card.Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
