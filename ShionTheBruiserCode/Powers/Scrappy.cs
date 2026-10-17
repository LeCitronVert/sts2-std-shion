using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class Scrappy() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public const int StrengthUp = 1;
    public const int DefenseUp = 1;
    public const int EnergyUp = 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar("EnergyUp", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];
    
    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
            return;
        
        this.Flash();
        await GainCardBonus(cardPlay);
        await PowerCmd.Decrement(this);
    }
    
    private async Task GainCardBonus(CardPlay cardPlay)
    {
        var cardBeingPlayed = cardPlay.Card;
        
        if (cardBeingPlayed.Type == CardType.Attack)
        {
            // Gain one temp strength
            _ = (await PowerCmd.Apply<ScrappyStrength>(new ThrowingPlayerChoiceContext(), Owner, StrengthUp, Owner, null!))!;
            return;
        }
        
        if (cardBeingPlayed.Type == CardType.Skill)
        {
            // Gain one temp defense
            await PowerCmd.Apply<ScrappyDexterity>(new ThrowingPlayerChoiceContext(), Owner, DefenseUp, Owner, null!);
            return;
        }
        
        if (Owner.Player == null) return;
        await PlayerCmd.GainEnergy(EnergyUp, Owner.Player);
    }
}