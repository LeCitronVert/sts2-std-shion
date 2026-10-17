using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class SlalIncomingBeam() : SlalTheEndlessCard(0,
    CardType.Attack, CardRarity.Token,
    TargetType.AllEnemies)
{
    public const int MaxEnergy = 10;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Confusion>(30),
        new DynamicVar("MaxEnergy", MaxEnergy),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<Confusion>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
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

        foreach (var enemy in enemies.Where(e => e.HasPower<WeakPower>() && e.Block > 0))
        {
            await CreatureCmd.LoseBlock(enemy, enemy.Block);
        }

        await SlalMechanicsHelper.ApplyToAllEnemies<Confusion>(choiceContext, card.Owner.Creature, card.DynamicVars["Confusion"].BaseValue, card);

        var energy = Math.Min(card.Owner.Creature.GetPowerAmount<WeakPower>(), MaxEnergy);
        if (energy > 0)
        {
            await PlayerCmd.GainEnergy(energy, card.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Confusion"].UpgradeValueBy(10);
    }
}
