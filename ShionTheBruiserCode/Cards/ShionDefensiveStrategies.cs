using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Counter the next 4 hindrances. Extend Guard and Mighty Guard by 1 (2) turn(s). Defensive.
/// </summary>
public class ShionDefensiveStrategies() : ShionDefensiveCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<CounterStrategy>(4),
        new DynamicVar("Extension", 1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.Defensive),
        HoverTipFactory.FromPower<Guard>(),
        HoverTipFactory.FromPower<MightyGuard>(),
        HoverTipFactory.FromPower<CounterStrategy>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var owner = card.Owner.Creature;

        await PowerCmd.Apply<CounterStrategy>(
            choiceContext,
            owner,
            card.DynamicVars["CounterStrategy"].BaseValue,
            owner,
            card
        );

        var extension = card.DynamicVars["Extension"].BaseValue;
        if (owner.HasPower<Guard>())
        {
            await PowerCmd.Apply<Guard>(choiceContext, owner, extension, owner, card);
        }
        if (owner.HasPower<MightyGuard>())
        {
            await PowerCmd.Apply<MightyGuard>(choiceContext, owner, extension, owner, card);
        }

        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Extension"].UpgradeValueBy(1);
    }
}
