using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Regent Team Blast. Forge 10, plus as much as ALL players' (upgraded: EVERYONE's, enemies included) Strength.
/// Then play your Sovereign Blade from anywhere.
/// </summary>
[Pool(typeof(RegentCardPool))]
public class RegentUncontestedReign() : TeamBlastCard(CardType.Attack, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new ForgeVar(10),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ForgeAmount").WithMultiplier((card, _) => ((RegentUncontestedReign)card).PreviewForge()),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        ..HoverTipFactory.FromForge(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    private IEnumerable<Creature> StrengthSources(ICombatState combatState) => IsUpgraded
        ? combatState.Allies.Concat(combatState.Enemies)
        : combatState.GetTeammatesOf(Owner.Creature).Where(c => c.IsPlayer);

    private decimal GetForgeAmount()
    {
        if (CombatState is null) return DynamicVars.Forge.BaseValue;
        return DynamicVars.Forge.BaseValue + StrengthSources(CombatState).Where(c => c.IsAlive).Sum(c => c.GetPowerAmount<StrengthPower>());
    }

    /// <summary>Preview only.</summary>
    private decimal PreviewForge() => CombatState is null ? 0 : Math.Max(0, GetForgeAmount());

    protected override async Task OnTeamBlast(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var blades = (await ForgeCmd.Forge(Math.Max(0, GetForgeAmount()), Owner, this)).ToList();
        var blade = blades.FirstOrDefault(b => b.Pile?.Type != PileType.Play);
        if (blade == null || CombatState is null) return;

        var target = play.Target is { IsAlive: true } ? play.Target : CombatState.HittableEnemies.FirstOrDefault();
        await CardPileCmd.Add(blade, PileType.Play);
        await CardCmd.AutoPlay(choiceContext, blade, target);
    }

    protected override void OnUpgrade()
    {
    }
}
