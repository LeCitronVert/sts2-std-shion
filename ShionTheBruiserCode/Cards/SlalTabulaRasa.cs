using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalTabulaRasa() : SlalTheEndlessCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public const int RandomDropsTo = 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Powers.Random>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);

        foreach (var other in ShionMechanicsHelper.GetAllCombatCards(card.Owner).ToList())
        {
            other.AddKeyword(SlalCardKeywords.Novel);
        }

        var random = card.Owner.Creature.GetPower<Powers.Random>();
        if (random != null)
        {
            await random.DropTo(choiceContext, RandomDropsTo);
        }

        var hand = PileType.Hand.GetPile(card.Owner).Cards.ToList();
        if (IsUpgraded)
        {
            foreach (var other in hand.Where(c => c.IsUpgradable))
            {
                CardCmd.Upgrade(other);
            }
        }
        foreach (var other in hand.Where(c => !c.EnergyCost.CostsX))
        {
            other.SetToFreeThisTurn();
        }
    }

    protected override void OnUpgrade()
    {
    }
}
