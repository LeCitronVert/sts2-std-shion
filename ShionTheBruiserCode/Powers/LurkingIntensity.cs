using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

public class LurkingIntensity() : ShionTheBruiserPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var source = this;
        if (player != source.Owner.Player)
            return;
        
        var colorlessOptions = ModelDb.CardPool<ColorlessCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).ToList();

        List<CardModel> selection = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Draw.GetPile(player),
            player,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 0, DynamicVars.Cards.IntValue)
        )).ToList();
        
        foreach (CardModel original in (await CardSelectCmd.FromHand(choiceContext, player, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, source.Amount), null, source)).ToList())
        {
            await CardCmd.Transform(
                new CardTransformation(original, colorlessOptions).Yield(),
                player.RunState.Rng.CombatCardSelection
            );
        }
    }
}