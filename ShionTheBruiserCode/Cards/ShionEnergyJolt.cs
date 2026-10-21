using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionEnergyJolt() : ShionTheBruiserCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public const int GalvanisedThreshold = 7;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(3, ValueProp.Move),
        new DynamicVar("DebuffAmount", 2)
    ];
    
    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<DebilitatePower>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
    ];
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsGalvanised(this.Owner.Creature, GalvanisedThreshold);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.TriggerAnim(
            card.Owner.Creature,
            Character.ShionTheBruiser.GetEnergyCastAnimIfApplicable(card.Owner.Character),
            Character.ShionTheBruiser.GetEnergyCastDelayIfApplicable(card.Owner.Character)
        );
        // Checked before the Attack resolves, since it consumes Vigor
        var isGalvanised = ShionMechanicsHelper.IsGalvanised(card.Owner.Creature, GalvanisedThreshold);
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .Targeting(play.Target)
            .WithEnergyBlast(card, "vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            play.Target,
            DynamicVars["DebuffAmount"].BaseValue,
            card.Owner.Creature,
            card
        );
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            play.Target,
            DynamicVars["DebuffAmount"].BaseValue,
            card.Owner.Creature,
            card
        );
        
        if (isGalvanised)
        {
            await PowerCmd.Apply<DebilitatePower>(
                choiceContext,
                play.Target,
                DynamicVars["DebuffAmount"].BaseValue,
                card.Owner.Creature,
                card
            );
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DebuffAmount"].UpgradeValueBy(1);
    }
}