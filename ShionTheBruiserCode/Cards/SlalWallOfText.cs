using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalWallOfText(): SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.RandomEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Confusion>()];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("DebuffAmount").WithMultiplier((model, _) => 
            PileType.Discard.GetPile(model.Owner).Cards.Count
        ),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var cardSource = this;
        
        await CreatureCmd.TriggerAnim(cardSource.Owner.Creature, "Cast", cardSource.Owner.Character.CastAnimDelay);
        var enemy = cardSource.Owner.RunState.Rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>) cardSource.CombatState?.HittableEnemies! ?? Array.Empty<Creature>());
        if (enemy != null)
        {
            await PowerCmd.Apply<Confusion>(
                choiceContext, 
                enemy,
                ((CalculatedVar) cardSource.DynamicVars["DebuffAmount"]).Calculate(enemy), 
                cardSource.Owner.Creature, 
                cardSource
            );
        }
    }

    protected override void OnUpgrade()
    {
    }
    
    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType pileTypeForCardPlay = base.GetResultPileTypeForCardPlay();
        if (IsUpgraded &&  pileTypeForCardPlay == PileType.Discard)
        {
            return PileType.Draw;
        }
        return pileTypeForCardPlay;
    }
}