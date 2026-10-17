using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain Block. Increase the Block of ALL Blooper cards this combat. Add a copy of this card into your Draw Pile.
/// </summary>
public class SlalBlooper() : SlalTheEndlessCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    private decimal _extraBlockFromBloopers;

    /// <summary>
    /// Required so we can restore the extra block amount after a downgrade (see Maul).
    /// </summary>
    private decimal ExtraBlockFromBloopers
    {
        get => _extraBlockFromBloopers;
        set
        {
            AssertMutable();
            _extraBlockFromBloopers = value;
        }
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(4M, ValueProp.Move),
        new DynamicVar("Increase", 1),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);

        var increase = card.DynamicVars["Increase"].BaseValue;
        foreach (var blooper in (card.Owner.PlayerCombatState?.AllCards ?? []).OfType<SlalBlooper>().ToList())
        {
            blooper.BuffFromBlooperPlay(increase);
        }

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card.CreateClone(), PileType.Draw, card.Owner, CardPilePosition.Random));
    }

    private void BuffFromBlooperPlay(decimal extraBlock)
    {
        DynamicVars.Block.BaseValue += extraBlock;
        ExtraBlockFromBloopers += extraBlock;
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Block.BaseValue += ExtraBlockFromBloopers;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(1);
    }
}
