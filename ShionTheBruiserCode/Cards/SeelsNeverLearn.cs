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
/// If you have more Blood than you have HP, gain 4 Max HP. Blood Bend X. Gain X Strength. Exhaust. (Retain.)
/// </summary>
public class SeelsNeverLearn() : SeelsTheRecklessCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new MaxHpVar(4M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("StrengthAmount").WithMultiplier((card, _) => SeelsMechanicsHelper.PreviewBloodBendX(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend),
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    private bool IsActive => SeelsMechanicsHelper.GetBlood(Owner.Creature) > Owner.Creature.CurrentHp;

    protected override bool ShouldGlowGoldInternal => IsActive;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // Checked before Blood Bending, which consumes the Blood.
        if (IsActive)
        {
            await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.MaxHp.BaseValue);
        }

        var x = await SeelsMechanicsHelper.BloodBend(choiceContext, this);
        if (x > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, x, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
