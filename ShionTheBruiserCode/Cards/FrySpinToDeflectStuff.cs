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
/// Lose 3 (2) Strength this turn. Discard your Hand. Gain 4 (6) Block for each card discarded. Draw 4 cards.
/// </summary>
public class FrySpinToDeflectStuff() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(3M),
        new BlockVar(4M, ValueProp.Move),
        new CardsVar(4),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        await PowerCmd.Apply<SpinToDeflectStuffStrengthDown>(choiceContext, creature, DynamicVars.Strength.BaseValue, creature, this);

        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (hand.Count > 0)
        {
            await CardCmd.Discard(choiceContext, hand);
            for (var i = 0; i < hand.Count; i++)
            {
                await CreatureCmd.GainBlock(creature, DynamicVars.Block, play);
            }
        }

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(-1M);
        DynamicVars.Block.UpgradeValueBy(2M);
    }
}
