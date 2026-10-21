using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Until your next turn, gain Hardened Shell equal to your HP minus 1. Refuse 1. Exhaust. (Retain.)
/// </summary>
public class FryLastChanceHitPoint() : FryTheCommanderCard(2, //todo : choose cost
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<Refuse>(1M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<HardenedShellPower>(),
        HoverTipFactory.FromPower<Refuse>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        var shell = (TemporaryHardenedShell)ModelDb.Power<TemporaryHardenedShell>().ToMutable();
        shell.Cap = Math.Max(0, creature.CurrentHp - 1);
        await PowerCmd.Apply(choiceContext, shell, creature, 1, creature, this);
        await FryMechanicsHelper.Refuse(choiceContext, creature, DynamicVars["Refuse"].BaseValue, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
