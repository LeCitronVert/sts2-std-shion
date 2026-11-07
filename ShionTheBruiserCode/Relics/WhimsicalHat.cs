using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Event relic, any character (no event gives it yet, so it can't drop for now).
/// At the start of combat, shuffle 2 Dazed into your Draw Pile.
/// At the end of your turn, play a random Ethereal card in your Hand for free (autoplay, like Stampede).
/// </summary>
[Pool(typeof(EventRelicPool))]
public class WhimsicalHat : ShionTheBruiserRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Dazed>(),
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
    ];

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.CombatState is null) return;

        Flash();
        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var dazed = Owner.Creature.CombatState.CreateCard<Dazed>(Owner);
            CardCmd.PreviewCardPileAdd(
                await CardPileCmd.AddGeneratedCardToCombat(dazed, PileType.Draw, Owner, CardPilePosition.Random)
            );
        }
    }

    public override async Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        var ethereal = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Keywords.Contains(CardKeyword.Ethereal) && !c.Keywords.Contains(CardKeyword.Unplayable))
            .ToList();
        var card = Owner.RunState.Rng.Shuffle.NextItem(ethereal);
        if (card == null) return;

        Flash();
        await CardCmd.AutoPlay(choiceContext, card, null);
    }
}
