using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Choose another player. Choose a card to add to both of your Hands (upgraded: upgraded card).
/// The options depend on the characters present: each one's own option, plus The Münchausen (default) always.
/// </summary>
public class FryAnythingYouCanDo() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyAlly)
{
    public override int CanonicalStarCost => 2;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    /// <summary>The option of a character, or null when it has none (only the default then).</summary>
    private static CardModel? OptionFor(Player player) => player.Character switch
    {
        Regent => ModelDb.Card<FryBfs>(),
        Defect => ModelDb.Card<FryCastingAShadow>(),
        SlalTheEndless => ModelDb.Card<FryNarrativeShapeshifting>(),
        SeelsTheReckless => ModelDb.Card<FrySignatureLaugh>(),
        Ironclad => ModelDb.Card<FryComebackMechanic>(),
        Silent => ModelDb.Card<FryDiagonalSpeedBoost>(),
        Necrobinder => ModelDb.Card<FryFriendlyGhost>(),
        global::ShionTheBruiser.ShionTheBruiserCode.Character.ShionTheBruiser => ModelDb.Card<FryHealThyself>(),
        FryTheCommander => ModelDb.Card<FryICanDoBetter>(),
        _ => null,
    };

    private static List<CardModel> Options(Player player) => player.RunState.Players
        .Select(OptionFor)
        .OfType<CardModel>()
        .Append(ModelDb.Card<FryTheMunchausen>())
        .DistinctBy(c => c.Id)
        .ToList();

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        Options(Owner).Select(c => HoverTipFactory.FromCard(c, IsUpgraded)).ToList();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target?.Player);
        if (CombatState is null) return;
        var partner = play.Target.Player;

        var choices = Options(Owner).Select(c =>
        {
            var choice = CombatState.CreateCard(c, Owner);
            if (IsUpgraded) CardCmd.Upgrade(choice);
            return choice;
        }).ToList();
        var chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, Owner, canSkip: false);
        if (chosen is null) return;

        foreach (var player in new[] { Owner, partner }.Distinct())
        {
            var copy = CombatState.CreateCard(chosen.CanonicalInstance, player);
            if (IsUpgraded) CardCmd.Upgrade(copy);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, player);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
