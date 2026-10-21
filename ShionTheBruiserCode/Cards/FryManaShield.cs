using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Cooldown 2. Gain 15 (20) Block. Lose 1 Dexterity.
/// </summary>
public class FryManaShield() : FryCooldownCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override int CanonicalStarCost => 2;

    public override bool GainsBlock => true;

    protected override int BaseCooldown => 2;

    protected override IEnumerable<DynamicVar> CooldownCardVars => [
        new BlockVar(15M, ValueProp.Move),
        new PowerVar<DexterityPower>(1M),
    ];

    protected override async Task OnCooldownCardPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, -DynamicVars.Dexterity.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5M);
    }
}
