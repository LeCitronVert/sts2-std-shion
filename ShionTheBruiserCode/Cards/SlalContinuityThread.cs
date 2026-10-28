using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Memory. You choose the cards Memory plays from your Discard Pile, but they are Exhausted. Ethereal (upgrade: not Ethereal).
/// </summary>
public class SlalContinuityThread() : SlalTheEndlessCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<Memory>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Memory>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", card.Owner.Character.CastAnimDelay);
        await SlalMechanicsHelper.GainMemory(choiceContext, card.Owner.Creature, card.DynamicVars["Memory"].BaseValue, card);
        await PowerCmd.Apply<ContinuityThread>(choiceContext, card.Owner.Creature, 1, card.Owner.Creature, card);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
