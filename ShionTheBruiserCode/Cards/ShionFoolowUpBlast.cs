#if STS2_BETA
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Visuals;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Token, never added to a pile: the Energy card Fool-ow Up Attack's blast comes from, so it counts as an Energy Attack
/// (Energy Damage Up, Spikes Gloves...). Its damage is set to the Arrow's base damage right before the blast.
/// Beta only.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class ShionFoolowUpBlast() : ShionTheBruiserCard(0,
    CardType.Attack, CardRarity.Token,
    TargetType.AnyEnemy)
{
    // Same art as Fool-ow Up Attack
    public override string CustomPortraitPath => "shion_foolow_up_attack.png".BigCardImagePath();
    public override string PortraitPath => "shion_foolow_up_attack.png".CardImagePath();

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override int MaxUpgradeLevel => 0;

    protected override HashSet<CardTag> CanonicalTags => [ShionCardTags.Energy];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(0M, ValueProp.Move)];

    public void SetDamage(decimal damage) => DynamicVars.Damage.BaseValue = damage;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithEnergyBlast(this, "vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }
}
#endif
