using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// If you have Block, gain Hardened Shell 0 for 1 turn. Lose 5 (3) Dexterity. Exhaust.
/// </summary>
public class FryBlockingStopsAllDamage() : FryTheCommanderCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DexterityPower>(5M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<HardenedShellPower>()];

    protected override bool ShouldGlowGoldInternal => Owner.Creature.Block > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var creature = Owner.Creature;
        if (creature.Block > 0)
        {
            var shell = (TemporaryHardenedShell)ModelDb.Power<TemporaryHardenedShell>().ToMutable();
            shell.Cap = 0;
            await PowerCmd.Apply(choiceContext, shell, creature, 1, creature, this);
        }
        await PowerCmd.Apply<DexterityPower>(choiceContext, creature, -DynamicVars.Dexterity.BaseValue, creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Dexterity.UpgradeValueBy(-2M);
    }
}
