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
/// Gain 1 Vigor for each Attack in your Discard Pile. Add The Door Slams You (+) into your Hand.
/// </summary>
public class FryDoorSlamOfRage() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromCard<FryTheDoorSlamsYou>(IsUpgraded),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState is null) return;

        var attacks = PileType.Discard.GetPile(Owner).Cards.Count(c => c.Type == CardType.Attack);
        if (attacks > 0)
        {
            await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, attacks, Owner.Creature, this);
        }

        var door = CombatState.CreateCard<FryTheDoorSlamsYou>(Owner);
        if (IsUpgraded) CardCmd.Upgrade(door);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(door, PileType.Hand, Owner));
    }

    protected override void OnUpgrade()
    {
    }
}
