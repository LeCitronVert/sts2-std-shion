using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 8 (10) Block. Decrease this card's Block by 1 this combat (like Steam Barrier).
/// Whenever you play a Form Power, put this into your Hand.
/// </summary>
public class SlalReactiveShape() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    private decimal _blockLostFromPlays;

    /// <summary>
    /// Required so we can restore the lost block amount after a downgrade (see Rampage).
    /// </summary>
    private decimal BlockLostFromPlays
    {
        get => _blockLostFromPlays;
        set
        {
            AssertMutable();
            _blockLostFromPlays = value;
        }
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8M, ValueProp.Move),
        new DynamicVar("Decrease", 1),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        var decrease = Math.Min(DynamicVars["Decrease"].BaseValue, DynamicVars.Block.BaseValue);
        DynamicVars.Block.BaseValue -= decrease;
        BlockLostFromPlays += decrease;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || !SlalMechanicsHelper.IsFormPower(cardPlay.Card) || !cardPlay.IsLastInSeries) return;
        if (Pile?.Type is not (PileType.Draw or PileType.Discard or PileType.Hand)) return;

        if (Pile.Type != PileType.Hand)
        {
            await CardPileCmd.Add(this, PileType.Hand);
        }
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Block.BaseValue -= BlockLostFromPlays;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2M);
    }
}
