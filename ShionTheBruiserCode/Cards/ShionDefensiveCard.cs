using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public abstract class ShionDefensiveCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    ShionTheBruiserCard(cost, type, rarity, target)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Defensive];
    
    protected async Task ApplyDefensiveToPlayer()
    {
        await PowerCmd.Apply<DefensiveDebuff>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            amount: 1,
            applier: Owner.Creature,
            cardSource: this
        );
    }

    // Defensive cards aren't playable in a turn where a Berserk or Galvanised effect triggered
    protected override bool IsPlayable => !ShionMechanicsHelper.SpecialEffectTriggeredThisTurn(Owner.Creature);
}