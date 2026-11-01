using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Necrobinder): double your partner's Osty's Max HP, then Summon an Osty of your own with as much Max HP as theirs. Exhaust.
/// </summary>
public class SeelsOstysBestie() : SeelsTeamUpCard(3,
    CardType.Skill,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Necrobinder>();

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("OstyHp").WithMultiplier((card, _) => ((SeelsOstysBestie)card).PreviewPartnerOstyHp()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.SummonStatic)];

    private static Player? Necrobinder(Player player) => player.Character is Necrobinder ? player : null;

    /// <summary>
    /// Preview only: the partner Osty's Max HP once doubled. On Seels' card, the first living Necrobinder ally's.
    /// </summary>
    private int PreviewPartnerOstyHp()
    {
        if (CombatState is null) return 0;
        var necrobinder = Initiator != null
            ? Owner
            : Owner.RunState.Players.FirstOrDefault(p => p != Owner && p.Character is Necrobinder && p.IsOstyAlive);
        return necrobinder is { IsOstyAlive: true } ? necrobinder.Osty!.MaxHp * 2 : 0;
    }

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        if (!partner.IsOstyAlive) return;

        var osty = partner.Osty!;
        await CreatureCmd.GainMaxHp(osty, osty.MaxHp);
        await OstyCmd.Summon(choiceContext, initiator, osty.MaxHp, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
