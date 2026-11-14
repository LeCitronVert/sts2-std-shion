using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (anyone), Defect: whenever you Evoke an Orb, your partner Channels a copy of it.
/// Whenever your partner plays a Power, gain 1 Focus. Ethereal (upgraded: not Ethereal).
/// </summary>
public class DefectConnect() : DefectTeamUpCard(2,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel? PartnerCharacter => null;

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<FocusPower>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Evoke),
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromPower<FocusPower>(),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        await TeamUpPower.Apply<Connect>(choiceContext, initiator.Creature, partner.Creature, 1, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
