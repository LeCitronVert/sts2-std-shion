using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Gain 1 Random. Deal 4 (5) damage. Add a copy of this card into your Draw Pile (upgrade: and one into your Discard Pile).
/// </summary>
public class SlalRunningJoke(): SlalTheEndlessCard(0,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Powers.Random>(1),
        new DamageVar(4M, ValueProp.Move),
        new CardsVar("DrawCard", 1),
        new CardsVar("DiscardCard", 0),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Powers.Random>(),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);

        await SlalMechanicsHelper.GainRandom(choiceContext, card.Owner.Creature, card.DynamicVars["Random"].BaseValue, card);

        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        for (int i = 0; i < DynamicVars["DrawCard"].BaseValue; i++)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card.CreateClone(), PileType.Draw, card.Owner), 2.2f);
        }

        for (int i = 0; i < DynamicVars["DiscardCard"].BaseValue; i++)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card.CreateClone(), PileType.Discard, card.Owner), 2.2f);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1M);
        DynamicVars["DiscardCard"].UpgradeValueBy(1);
    }
}
