using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain Block. Whenever you play a Form Power, put this into your Hand and increase its Block this combat.
/// </summary>
public class SlalReactiveShape() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    private decimal _extraBlockFromForms;

    /// <summary>
    /// Required so we can restore the extra block amount after a downgrade (see Rampage).
    /// </summary>
    private decimal ExtraBlockFromForms
    {
        get => _extraBlockFromForms;
        set
        {
            AssertMutable();
            _extraBlockFromForms = value;
        }
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5M, ValueProp.Move),
        new DynamicVar("Increase", 1),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || !SlalMechanicsHelper.IsFormPower(cardPlay.Card) || !cardPlay.IsLastInSeries) return;
        if (Pile?.Type is not (PileType.Draw or PileType.Discard or PileType.Hand)) return;

        var increase = DynamicVars["Increase"].BaseValue;
        DynamicVars.Block.BaseValue += increase;
        ExtraBlockFromForms += increase;

        if (Pile.Type != PileType.Hand)
        {
            await CardPileCmd.Add(this, PileType.Hand);
        }
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Block.BaseValue += ExtraBlockFromForms;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(1);
    }
}
