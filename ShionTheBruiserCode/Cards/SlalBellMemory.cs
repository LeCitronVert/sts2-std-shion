using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalBellMemory(): SlalTheEndlessCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6M, ValueProp.Move),
        new RepeatVar(2)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext)
        ;
        CardModel? card2 = PileType.Discard.GetPile(card.Owner).Cards.Where<CardModel>((Func<CardModel, bool>) (c => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable))).ToList<CardModel>().StableShuffle<CardModel>(card.Owner.RunState.Rng.Shuffle).FirstOrDefault<CardModel>() ?? PileType.Discard.GetPile(card.Owner).Cards.Where<CardModel>((Func<CardModel, bool>) (c => c.Type == CardType.Attack)).ToList<CardModel>().StableShuffle<CardModel>(card.Owner.RunState.Rng.Shuffle).FirstOrDefault<CardModel>();
        if (card2 == null)
            return;
        await CardCmd.AutoPlay(choiceContext, card2, null);
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.Damage.UpgradeValueBy(2M);
    }
}