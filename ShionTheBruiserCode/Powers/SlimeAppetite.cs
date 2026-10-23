using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Each time you create a Slimed, gain X Block. Whenever you play a Slimed, gain Y Dexterity this turn.
/// Whenever you play a Slimy card, apply Z Weak to a random enemy and gain Z Plating.
/// Amount is the Block, the "Dexterity" and "SlimyBonus" vars are stacked by the card.
/// </summary>
public class SlimeAppetite : SlalTheEndlessPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Dexterity", 0),
        new DynamicVar("SlimyBonus", 0),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromKeyword(SlalCardKeywords.Slimy),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
    ];

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card is not Slimed || card.Owner != Owner.Player) return;

        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player) return;

        if (cardPlay.Card is Slimed)
        {
            var dexterity = DynamicVars["Dexterity"].BaseValue;
            if (dexterity > 0)
            {
                Flash();
                await PowerCmd.Apply<SlimeAppetiteDexterity>(choiceContext, Owner, dexterity, Owner, null);
            }
        }

        if (SlalMechanicsHelper.IsSlimy(cardPlay.Card) && CombatState is not null)
        {
            var bonus = DynamicVars["SlimyBonus"].BaseValue;
            if (bonus <= 0) return;

            Flash();
            var enemy = Owner.Player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
            if (enemy != null)
            {
                await PowerCmd.Apply<WeakPower>(choiceContext, enemy, bonus, Owner, null);
            }
            await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, bonus, Owner, null);
        }
    }
}

/// <summary>
/// Dexterity gained this turn from Slime Appetite.
/// </summary>
public class SlimeAppetiteDexterity : CustomTemporaryPowerModelWrapper<SlalSlimeAppetite, DexterityPower>;
