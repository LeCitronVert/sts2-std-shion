using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enchantments;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Maim 2. An enemy loses 2 HP. Add Maim 1 to a random Attack in your Hand
/// (to ALL Attacks in your Hand).
/// </summary>
public class SeelsStealBlood() : SeelsTheRecklessCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [SeelsCardKeywords.Maim];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        MaimHelper.Var(2M),
        new DynamicVar("EnemyHpLoss", 2M),
        new DynamicVar("AddedMaim", 1M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Blood>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Damage(choiceContext, play.Target, DynamicVars["EnemyHpLoss"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, Owner.Creature, this);

        var attacks = PileType.Hand.GetPile(Owner).Cards.Where(IsAttackToBuff).ToList();
        if (!IsUpgraded)
        {
            attacks = attacks.TakeRandom(1, Owner.RunState.Rng.CombatCardSelection).ToList();
        }

        foreach (var attack in attacks)
        {
            MaimHelper.AddMaim(attack, DynamicVars["AddedMaim"].IntValue);
        }
    }

    private bool IsAttackToBuff(CardModel card) => card != this && card.Type == CardType.Attack;

    protected override void OnUpgrade()
    {
    }
}
