using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Bloody Daggers (Team-Up), on Seels.
/// <list type="bullet">
/// <item>Shivs your Partner creates get the Blood Bend X keyword (<see cref="SeelsCardKeywords.BloodyShiv"/>):
/// they deal X additional damage, X being the Partner's Blood, which they then Blood Bend.</item>
/// <item>Whenever your Partner plays a Skill, they gain 1 Blood (times Amount).</item>
/// <item>Whenever your Partner Blood Bends X, you gain X Blood.</item>
/// </list>
/// </summary>
public class BloodyDaggers : TeamUpPower, ISeelsListener
{
    public const int BloodPerSkill = 1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Blood", BloodPerSkill)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Shiv>(),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodyShiv),
        HoverTipFactory.FromPower<Blood>(),
    ];

    private bool IsPartnerBloodyShiv(CardModel? card) =>
        card != null && card.Keywords.Contains(SeelsCardKeywords.BloodyShiv) && Partners.Contains(card.Owner.Creature);

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card is Shiv && creator != null && creator == card.Owner && Partners.Contains(creator.Creature)
            && !card.Keywords.Contains(SeelsCardKeywords.BloodyShiv))
        {
            card.AddKeyword(SeelsCardKeywords.BloodyShiv);
        }
        return Task.CompletedTask;
    }

    // The Shiv's X is the Partner's Blood when it hits; it is consumed once the Shiv has been played.
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!IsPartnerBloodyShiv(cardSource) || dealer != cardSource!.Owner.Creature || !props.IsPoweredAttack()) return 0m;
        return SeelsMechanicsHelper.GetBlood(dealer);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (Owner.IsDead || !Partners.Contains(card.Owner.Creature)) return;

        if (IsPartnerBloodyShiv(card))
        {
            await SeelsMechanicsHelper.BloodBend(choiceContext, card.Owner, card);
        }

        if (card.Type == CardType.Skill)
        {
            Flash();
            await SeelsMechanicsHelper.GainBlood(choiceContext, card.Owner.Creature, BloodPerSkill * Amount);
        }
    }

    public async Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source)
    {
        if (x <= 0 || Owner.IsDead || !Partners.Contains(player.Creature)) return;

        Flash();
        await SeelsMechanicsHelper.GainBlood(choiceContext, Owner, x);
    }
}
