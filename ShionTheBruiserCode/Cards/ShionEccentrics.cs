using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Slal): adds Novel to ALL of your cards. When you gain Random, your partner gains the same amount.
/// When your partner reaches 10 Random, you gain 1 Strength and Dexterity.
/// </summary>
public class ShionEccentrics() : ShionTeamUpCard(3,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<SlalTheEndless>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<Eccentrics>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SlalCardKeywords.Novel),
        HoverTipFactory.FromPower<Powers.Random>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        await SlalMechanicsHelper.EnsureCreatureHasNovelTracker(initiator.Creature);
        foreach (var card in ShionMechanicsHelper.GetAllCombatCards(initiator).ToList())
        {
            SlalMechanicsHelper.AddNovel(card);
        }

        await TeamUpPower.Apply<Eccentrics>(choiceContext, initiator.Creature, partner.Creature, DynamicVars["Eccentrics"].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
