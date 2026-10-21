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
/// Heal 3 HP for each card in your Hand. Discard your Hand.
/// </summary>
public class FryHealThyself() : FryTheCommanderCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override int CanonicalStarCost => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(3M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (hand.Count == 0) return;

        await CreatureCmd.Heal(Owner.Creature, hand.Count * DynamicVars.Heal.BaseValue);
        await CardCmd.Discard(choiceContext, hand);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
