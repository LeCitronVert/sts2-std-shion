using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergySurge() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public const int GalvanisedThreshold = 12;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(1),
        new PowerVar<VigorPower>(3),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
    ];
    
    // The Vigor gained by this card counts towards its Galvanised condition
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsGalvanised(this.Owner.Creature, GalvanisedThreshold, DynamicVars["VigorPower"].IntValue);
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.TriggerAnim(
            card.Owner.Creature,
            Character.ShionTheBruiser.GetEnergyCastAnimIfApplicable(card.Owner.Character),
            Character.ShionTheBruiser.GetEnergyCastDelayIfApplicable(card.Owner.Character)
        );

        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["VigorPower"].BaseValue,
            card.Owner.Creature,
            card)
        ;
        
        if (!ShionMechanicsHelper.TriggerGalvanised(card.Owner.Creature, GalvanisedThreshold))
        {
            return;
        }
        
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["StrengthPower"].BaseValue,
            card.Owner.Creature,
            card)
        ;
    }

    protected override void OnUpgrade()
    {
        this.EnergyCost.UpgradeBy(-1);
        this.DynamicVars["VigorPower"].UpgradeValueBy(3);
    }
}