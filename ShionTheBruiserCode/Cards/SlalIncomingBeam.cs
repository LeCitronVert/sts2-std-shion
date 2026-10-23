using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

using ShionTheBruiser.ShionTheBruiserCode.Helper;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Deal 10 damage to ALL enemies. Increase this card's damage by 2 (3) each time you create a card.
/// Add Reverse Beam (+) into your Discard Pile. Exhaust.
/// Only cards created after this one count (not its own creation, nor the Reverse Beam it adds after the damage).
/// </summary>
[Pool(typeof(TokenCardPool))]
public class SlalIncomingBeam() : SlalTheEndlessCard(0,
    CardType.Attack, CardRarity.Token,
    TargetType.AllEnemies)
{
    private decimal _extraDamageFromCreations;

    /// <summary>
    /// Required so we can restore the extra damage amount after a downgrade (see Rampage).
    /// </summary>
    private decimal ExtraDamageFromCreations
    {
        get => _extraDamageFromCreations;
        set
        {
            AssertMutable();
            _extraDamageFromCreations = value;
        }
    }

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10M, ValueProp.Move),
        new DynamicVar("Increase", 2),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<SlalReverseBeam>(IsUpgraded),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (card.CombatState is null) return;

        var enemies = card.CombatState.HittableEnemies.ToList();
        if (enemies.Count > 0)
        {
            await CreatureCmd.TriggerAnim(card.Owner.Creature, "Cast", 0.5f);
            var beam = NHyperbeamVfx.Create(card.Owner.Creature, enemies.Last());
            if (beam != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(beam);
                await Cmd.Wait(0.5f);
            }
        }

        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState)
            .WithNoAttackerAnim()
            .Execute(choiceContext);

        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
            SlalMechanicsHelper.CreateCard<SlalReverseBeam>(card.Owner, IsUpgraded), PileType.Discard, card.Owner));
    }

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card == this || card.Owner != Owner) return Task.CompletedTask;

        var increase = DynamicVars["Increase"].BaseValue;
        DynamicVars.Damage.BaseValue += increase;
        ExtraDamageFromCreations += increase;
        return Task.CompletedTask;
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Damage.BaseValue += ExtraDamageFromCreations;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(1);
    }
}
