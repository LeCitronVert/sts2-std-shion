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
/// Cooldown 2. Forge 15 (20). Put Sovereign Blade from ANYWHERE into your Hand.
/// </summary>
public class FryBfs() : FryCooldownCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override int CanonicalStarCost => 3;

    protected override int BaseCooldown => 2;

    protected override IEnumerable<DynamicVar> CooldownCardVars => [new ForgeVar(15)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SovereignBlade>()];

    protected override async Task OnCooldownCardPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ForgeCmd.Forge(DynamicVars.Forge.BaseValue, Owner, this);

        var blade = SeelsMechanicsHelper.GetAllCombatCards(Owner)
            .OfType<SovereignBlade>()
            .FirstOrDefault(c => c.Pile?.Type != PileType.Hand);
        if (blade is not null)
        {
            await CardPileCmd.Add(blade, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Forge.UpgradeValueBy(5M);
    }
}
