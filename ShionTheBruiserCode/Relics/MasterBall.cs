using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Shion's Orobas relic (ShionAncientRelicPatches). Gain Abby the Absol, Show of Strength and Press the Advantage.
/// Lose 20 Max HP. Abby the Absol can't be obtained any other way.
/// </summary>
[Pool(typeof(EventRelicPool))]
public class MasterBall : ShionTheBruiserRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(20)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        ..HoverTipFactory.FromRelic<AbbyTheAbsol>(),
        HoverTipFactory.FromCard<ShionShowOfStrength>(),
        HoverTipFactory.FromCard<ShionPressTheAdvantage>(),
    ];

    public override async Task AfterObtained()
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.HpLoss.BaseValue, isFromCard: false);
        await RelicCmd.Obtain(ModelDb.Relic<AbbyTheAbsol>().ToMutable(), Owner);

        List<CardPileAddResult> results = [
            await CardPileCmd.Add(Owner.RunState.CreateCard<ShionShowOfStrength>(Owner), PileType.Deck),
            await CardPileCmd.Add(Owner.RunState.CreateCard<ShionPressTheAdvantage>(Owner), PileType.Deck),
        ];
        CardCmd.PreviewCardPileAdd(results, 2f);
    }
}
