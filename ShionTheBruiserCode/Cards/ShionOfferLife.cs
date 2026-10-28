using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Necrobinder): you lose HP equal to your current HP -1, and gain 99 Weak.
/// Your partner Summons as much as the HP you lost. You can't lose HP this turn, and gain 3 Intangible next turn. Exhaust.
/// </summary>
public class ShionOfferLife() : ShionTeamUpCard(0,
    CardType.Skill,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Necrobinder>();

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(99),
        new PowerVar<IntangiblePower>(3),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.Static(StaticHoverTip.SummonStatic),
        HoverTipFactory.FromPower<IntangiblePower>(),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        var creature = initiator.Creature;
        var hpLost = creature.CurrentHp - 1;
        await SeelsMechanicsHelper.LoseHp(choiceContext, creature, hpLost, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, creature, DynamicVars.Weak.BaseValue, creature, this);

        if (hpLost > 0)
        {
            await OstyCmd.Summon(choiceContext, partner, hpLost, this);
        }

        await PowerCmd.Apply<OfferLife>(choiceContext, creature, DynamicVars["IntangiblePower"].BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
