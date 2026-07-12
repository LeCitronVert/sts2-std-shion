using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

[Pool(typeof(ColorlessCardPool))]
public class CosmoGuidance() : ShionTheBruiserCard(2,
    CardType.Power, CardRarity.Token,
    TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;


    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BuffAmount", 5)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, 
            card.Owner.Creature, 
            card.DynamicVars["BuffAmount"].BaseValue, 
            card.Owner.Creature, 
            card
        );
        await PowerCmd.Apply<DexterityPower>(
            choiceContext, 
            card.Owner.Creature, 
            card.DynamicVars["BuffAmount"].BaseValue, 
            card.Owner.Creature, 
            card
        );
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}