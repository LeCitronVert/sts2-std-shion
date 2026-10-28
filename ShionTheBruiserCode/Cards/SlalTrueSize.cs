using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Add 2 Slimed into your Discard Pile for each of your Slimy cards (Draw Pile, Hand and Discard Pile).
/// Deal damage equal to the number of cards in your Discard Pile. Exhaust.
/// </summary>
public class SlalTrueSize() : SlalTheEndlessCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public const int SlimedPerSlimyCard = 2;

    private static readonly PileType[] SlimyPiles = [PileType.Draw, PileType.Hand, PileType.Discard];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        // Counts the Slimed added first
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            PileType.Discard.GetPile(card.Owner).Cards.Count + CountSlimyCards(card) * SlimedPerSlimyCard),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Slimy),
        HoverTipFactory.FromCard<Slimed>(),
    ];

    private static int CountSlimyCards(CardModel card)
    {
        return SlimyPiles
            .SelectMany(pile => pile.GetPile(card.Owner).Cards)
            .Count(c => c != card && SlalMechanicsHelper.IsSlimy(c));
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        if (card.CombatState is null) return;

        var slimedCount = CountSlimyCards(card) * SlimedPerSlimyCard;
        List<CardModel> slimed = [];
        for (var i = 0; i < slimedCount; i++)
        {
            slimed.Add(card.CombatState.CreateCard<Slimed>(card.Owner));
        }
        if (slimed.Count > 0)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(slimed, PileType.Discard, card.Owner));
        }

        // Not the CalculatedDamage var: it would count the new Slimed twice
        var damage = PileType.Discard.GetPile(card.Owner).Cards.Count;
        await DamageCmd.Attack(damage)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
