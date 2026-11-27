using BaseLib.Utils;
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
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team Blast. Apply Shrink to ALL enemies. Request 1, 2 and 3. ALL players gain 7 Strength and 7 Dexterity.
/// </summary>
public class FryMechaExpansionPack() : FryTheCommanderCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllAllies)
{
    private static readonly int[] Requests = [1, 2, 3];

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.TeamBlast, FryCardKeywords.Request];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<ShrinkPower>(3M), //todo : Shrink amount not specified
        new PowerVar<StrengthPower>(7M),
        new PowerVar<DexterityPower>(7M),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ShrinkPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner.Creature;
        if (CombatState is not null)
        {
            await PowerCmd.Apply<ShrinkPower>(choiceContext, CombatState.HittableEnemies.ToList(), DynamicVars["ShrinkPower"].BaseValue, owner, this);
        }

        await FryMechanicsHelper.Request(choiceContext, this, Requests, async ctx =>
        {
            if (CombatState is null) return;
            foreach (var player in CombatState.GetTeammatesOf(owner).Where(c => c.IsPlayer && c.IsAlive).ToList())
            {
                await PowerCmd.Apply<StrengthPower>(ctx, player, DynamicVars.Strength.BaseValue, owner, this);
                await PowerCmd.Apply<DexterityPower>(ctx, player, DynamicVars.Dexterity.BaseValue, owner, this);
            }
        });

        await DreamersMechanicsHelper.ExhaustAllTeamBlasts(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
