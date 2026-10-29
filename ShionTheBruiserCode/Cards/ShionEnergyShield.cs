using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Turtle Hermit Shell starter. Gain 3 Block. Galvanised 2: Gain 3 additional Block, then lose 2 Vigor.
/// </summary>
public class ShionEnergyShield() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    public const int GalvanisedThreshold = 2;

    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend, ShionCardTags.Energy];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(3, ValueProp.Move),
        new DynamicVar("GalvanisedBlock", 3),
        new DynamicVar("VigorLoss", GalvanisedThreshold),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
    ];

    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsGalvanised(Owner.Creature, GalvanisedThreshold);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var owner = card.Owner.Creature;
        await CreatureCmd.GainBlock(owner, card.DynamicVars.Block, play);

        if (!ShionMechanicsHelper.TriggerGalvanised(owner, GalvanisedThreshold)) return;

        await CreatureCmd.GainBlock(owner, card.DynamicVars["GalvanisedBlock"].BaseValue, card.DynamicVars.Block.Props, play);
        // Fell Dragon makes it Galvanised without Vigor: never go below 0
        var vigorLoss = Math.Min(card.DynamicVars["VigorLoss"].BaseValue, owner.GetPowerAmount<VigorPower>());
        if (vigorLoss > 0)
        {
            await PowerCmd.Apply<VigorPower>(choiceContext, owner, -vigorLoss, owner, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["GalvanisedBlock"].UpgradeValueBy(1);
    }
}
