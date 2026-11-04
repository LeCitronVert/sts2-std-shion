using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (anyone): add 2 (3) Slashes into you and your partner's Hand, Draw Pile and Discard Pile. Exhaust.
/// </summary>
public class SeelsScratchinPals() : SeelsTeamUpCard(1,
    CardType.Skill,
    TargetType.Self)
{
    public override CharacterModel? PartnerCharacter => null;

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Slashes", 2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SeelsSlash>()];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        var amount = DynamicVars["Slashes"].IntValue;
        foreach (var player in new[] { initiator, partner })
        {
            await SeelsMechanicsHelper.AddSlashes(player, amount, PileType.Hand, false);
            await SeelsMechanicsHelper.AddSlashes(player, amount, PileType.Draw, false);
            await SeelsMechanicsHelper.AddSlashes(player, amount, PileType.Discard, false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Slashes"].UpgradeValueBy(1);
    }
}
