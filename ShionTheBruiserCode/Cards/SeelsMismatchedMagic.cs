using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Fry): when your partner spends Stars, convert 1 Blood into 2 HP.
/// When you Blood Bend, your partner converts 3 Stars into 10 Bad Faith.
/// </summary>
public class SeelsMismatchedMagic() : SeelsTeamUpCard(1,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<FryTheCommander>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Blood", MismatchedMagic.BloodConverted),
        new DynamicVar("Heal", MismatchedMagic.HealPerConversion),
        new DynamicVar("Stars", MismatchedMagic.StarsConverted),
        new DynamicVar("BadFaith", MismatchedMagic.BadFaithPerConversion),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<BadFaith>(),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        await TeamUpPower.Apply<MismatchedMagic>(choiceContext, initiator.Creature, partner.Creature, 1, this);
    }

    protected override void OnUpgrade()
    {
        //todo : upgrade not specified
    }
}
