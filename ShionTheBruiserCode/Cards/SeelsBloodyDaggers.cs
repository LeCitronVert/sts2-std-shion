using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Silent): whenever your partner creates a Shiv, give it Blood Bend X (+X damage).
/// Whenever they play a Skill, they gain 1 Blood. Whenever they Blood Bend X, gain the Blood they used.
/// </summary>
public class SeelsBloodyDaggers() : SeelsTeamUpCard(2,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Silent>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Blood", BloodyDaggers.BloodPerSkill)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Shiv>(),
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodyShiv),
        HoverTipFactory.FromPower<Blood>(),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        await TeamUpPower.Apply<BloodyDaggers>(choiceContext, initiator.Creature, partner.Creature, 1, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
