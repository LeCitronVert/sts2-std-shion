using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Silent): when you or your partner plays a card, give the other player 1 Scrappy.
/// </summary>
public class ShionFellowship() : ShionTeamUpCard(2,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Silent>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<Fellowship>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Scrappy>()];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        var amount = DynamicVars["Fellowship"].BaseValue;
        await TeamUpPower.Apply<Fellowship>(choiceContext, initiator.Creature, partner.Creature, amount, this);
        await TeamUpPower.Apply<Fellowship>(choiceContext, partner.Creature, initiator.Creature, amount, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
