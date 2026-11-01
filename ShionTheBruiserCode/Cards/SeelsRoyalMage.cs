using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Monsters;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Regent): you and your partner each summon a Blood Inklet with 10% (20%) of their own Max HP and 1 Slippery. Exhaust.
/// Blood Inklets take attack damage for their owner, before Osty (<see cref="BloodInkletPower"/>).
/// </summary>
public class SeelsRoyalMage() : SeelsTeamUpCard(2,
    CardType.Skill,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<Regent>();

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("MaxHpPercent", 10),
        new PowerVar<SlipperyPower>(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("InkletHp").WithMultiplier((card, _) => ((SeelsRoyalMage)card).PreviewOwnInkletHp()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<BloodInkletPower>(),
        HoverTipFactory.FromPower<SlipperyPower>(),
    ];

    private decimal InkletHp(Player player) =>
        Math.Max(1m, Math.Floor(player.Creature.MaxHp * DynamicVars["MaxHpPercent"].BaseValue / 100m));

    /// <summary>Preview only: the Blood Inklet this card's owner would summon.</summary>
    private decimal PreviewOwnInkletHp() => CombatState is null ? 0 : InkletHp(Owner);

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        var slippery = DynamicVars["SlipperyPower"].BaseValue;
        foreach (var player in new[] { initiator, partner })
        {
            await BloodInklet.Summon(choiceContext, player, InkletHp(player), slippery, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHpPercent"].UpgradeValueBy(10);
    }
}
