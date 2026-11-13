using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
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
/// Deal 20 (25) damage. Deals 3 (4) additional damage for each Vulnerable on the enemy.
/// Can only be played if it would kill the enemy (after its Block; see <see cref="Patches.BloodyExecutionTargetPatch"/>).
/// </summary>
public class SeelsBloodyExecution() : SeelsTheRecklessCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [SeelsCardTags.Blood];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(20M, ValueProp.Move),
        new DynamicVar("VulnerableDamage", 3M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    // Bonus per Vulnerable on the target (also shows in the targeted damage preview).
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource
#if STS2_BETA
        , CardPlay? cardPlay
#endif
    )
    {
        if (cardSource != this || target is null) return 0m;
        return SeelsMechanicsHelper.GetVulnerable(target) * DynamicVars["VulnerableDamage"].BaseValue;
    }

    /// <summary>Playable (and glowing) only while at least one enemy can be killed by it.</summary>
    protected override bool IsPlayable =>
        CombatState?.HittableEnemies.Any(WouldKill) ?? false;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    /// <summary>
    /// Whether this card's damage (with every modifier, as previewed) gets through the enemy's Block and kills it.
    /// Pure: only reads the damage hooks (no HP-loss hooks, some of them roll RNG).
    /// </summary>
    public bool WouldKill(Creature target)
    {
        if (CombatState is null || !target.IsAlive) return false;
        var damage = Sts2Compat.ModifyDamage(Owner.RunState, CombatState, target, Owner.Creature, DynamicVars.Damage.BaseValue,
            DynamicVars.Damage.Props, this, null, ModifyDamageHookType.All, CardPreviewMode.None, out _);
        return Math.Max(0m, Math.Floor(damage) - target.Block) >= target.CurrentHp;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5M);
        DynamicVars["VulnerableDamage"].UpgradeValueBy(1M);
    }
}
