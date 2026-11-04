using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 10 (12) damage. If you lost HP this turn, hits three times, and Maim 3.
/// </summary>
public class SeelsHighReward() : SeelsTheRecklessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy), IConditionalMaim
{
    public const int LostHpHits = 3;

    /// <summary>The conditional Maim is only active while the card is being played.</summary>
    private bool _maimActive;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SeelsCardKeywords.Maim];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10M, ValueProp.Move),
        new DynamicVar("LostHpMaim", 3M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("HitAmount").WithMultiplier((card, _) => GetHits(card)),
    ];

    /// <summary>Extra Maim from the "lost HP" condition, read by <see cref="MaimHelper.GetMaim"/>.</summary>
    public int ConditionalMaim => _maimActive ? DynamicVars["LostHpMaim"].IntValue : 0;

    private static int GetHits(CardModel card) => SeelsMechanicsHelper.LostHpThisTurn(card.Owner.Creature) ? LostHpHits : 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Blood>()];

    protected override bool ShouldGlowGoldInternal => SeelsMechanicsHelper.LostHpThisTurn(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var lostHp = SeelsMechanicsHelper.LostHpThisTurn(Owner.Creature);
        _maimActive = lostHp;
        try
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(play.Target)
                .WithHitCount(lostHp ? LostHpHits : 1)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        finally
        {
            _maimActive = false;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
    }
}
