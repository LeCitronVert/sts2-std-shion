using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Always Novel. Add 1 Steady Fresh Idea into another player's Hand.
/// Upgraded: add 2 Steady Fresh Ideas into ALL players' Hands (including yours).
/// </summary>
public class SlalEnthusiasticWave() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SlalCardKeywords.AlwaysNovel];

    public override TargetType TargetType => IsUpgraded ? TargetType.AllAllies : TargetType.AnyAlly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalFreshIdea>(),
        ..HoverTipFactory.FromEnchantment<Steady>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        List<Player> players;
        if (IsUpgraded)
        {
            players = card.CombatState.Allies.Where(c => c.IsAlive && c.Player != null).Select(c => c.Player!).ToList();
        }
        else
        {
            ArgumentNullException.ThrowIfNull(play.Target?.Player);
            players = [play.Target.Player];
        }

        foreach (var player in players)
        {
            for (var i = 0; i < card.DynamicVars.Cards.IntValue; i++)
            {
                // Created for the receiving player, so it goes into their Hand
                var freshIdea = SlalMechanicsHelper.CreateCard<SlalFreshIdea>(player);
                SlalMechanicsHelper.MakeSteady(freshIdea);
                await CardPileCmd.AddGeneratedCardToCombat(freshIdea, PileType.Hand, player);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
