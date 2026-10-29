using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// At the start of combat, add 2 Slimed to your draw pile. At the start of your turn, gain 1 Free Status.
/// </summary>
public class AppleSauce : SlalTheEndlessRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2),
        new PowerVar<FreeStatus>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.FromPower<FreeStatus>(),
    ];

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.CombatState is null) return;

        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var slimed = Owner.Creature.CombatState.CreateCard<Slimed>(Owner);
            CardCmd.PreviewCardPileAdd(
                await CardPileCmd.AddGeneratedCardToCombat(slimed, PileType.Draw, Owner, CardPilePosition.Random)
            );
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        Flash();
        await PowerCmd.Apply<FreeStatus>(choiceContext, Owner.Creature, DynamicVars["FreeStatus"].BaseValue, Owner.Creature, null);
    }
}
