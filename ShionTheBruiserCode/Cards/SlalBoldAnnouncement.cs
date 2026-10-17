using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalBoldAnnouncement() : SlalTheEndlessCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(7M, ValueProp.Move),
        new PowerVar<ThornsPower>(1),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ThornsPower>()];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        var enemyCount = card.CombatState.HittableEnemies.Count;
        var slalPowers = ModelDb.CardPool<SlalTheEndlessCardPool>()
            .GetUnlockedCards(card.Owner.UnlockState, card.Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Power);

        List<CardModel> powers = [];
        for (var i = 0; i < enemyCount; i++)
        {
            await CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
            await PowerCmd.Apply<ThornsPower>(choiceContext, card.Owner.Creature, card.DynamicVars["ThornsPower"].BaseValue, card.Owner.Creature, card);
            powers.AddRange(CardFactory.GetForCombat(card.Owner, slalPowers, 1, card.Owner.RunState.Rng.CombatCardGeneration));
        }

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(powers, PileType.Discard, card.Owner));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1M);
        DynamicVars["ThornsPower"].UpgradeValueBy(1);
    }
}
