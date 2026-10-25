using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 3 (4) Dexterity this turn. Until your next turn, whenever another player would take Attack damage
/// from an enemy, you take it instead.
/// </summary>
public class SlalInstantMattress() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DexterityPower>(3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<InstantMattress>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var owner = card.Owner.Creature;
        await PowerCmd.Apply<InstantMattressDexterity>(choiceContext, owner, card.DynamicVars.Dexterity.BaseValue, owner, card);
        await PowerCmd.Apply<InstantMattress>(choiceContext, owner, 1, owner, card);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Dexterity.UpgradeValueBy(1);
    }
}
