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
/// Refuse 2 (3). Gain 40 Bad Faith. After 2 (3) turns, lose ALL Bad Faith.
/// </summary>
public class FryUltimateForm() : FryTheCommanderCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [SlalCardTags.Form];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Refuse>(2M),
        new PowerVar<BadFaith>(40M),
        new PowerVar<UltimateForm>(2M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Refuse>(),
        HoverTipFactory.FromPower<BadFaith>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await FryMechanicsHelper.Refuse(choiceContext, creature, DynamicVars["Refuse"].BaseValue, this);
        await FryMechanicsHelper.GainBadFaith(choiceContext, creature, DynamicVars["BadFaith"].BaseValue, this);
        await PowerCmd.Apply<UltimateForm>(choiceContext, creature, DynamicVars["UltimateForm"].BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Refuse"].UpgradeValueBy(1M);
        DynamicVars["UltimateForm"].UpgradeValueBy(1M);
    }
}
