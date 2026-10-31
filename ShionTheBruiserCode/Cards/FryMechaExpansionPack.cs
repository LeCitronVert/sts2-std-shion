using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team Blast. Request 2 (1). Apply Shrink to ALL enemies. ALL players gain 7 Strength and 7 Dexterity for 3 turns.
/// </summary>
public class FryMechaExpansionPack() : FryTheCommanderCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllAllies)
{
    public const int Duration = 3;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [DreamersKeywords.TeamBlast, FryCardKeywords.Request];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Request", 2M),
        new PowerVar<ShrinkPower>(3M), //todo : Shrink amount not specified
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ShrinkPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner.Creature;
        await FryMechanicsHelper.Request(choiceContext, this, DynamicVars["Request"].IntValue, async ctx =>
        {
            if (CombatState is null) return;
            await PowerCmd.Apply<ShrinkPower>(ctx, CombatState.HittableEnemies.ToList(), DynamicVars["ShrinkPower"].BaseValue, owner, this);
            foreach (var player in CombatState.GetTeammatesOf(owner).Where(c => c.IsPlayer && c.IsAlive).ToList())
            {
                await PowerCmd.Apply<MechaExpansionPack>(ctx, player, Duration, owner, this);
            }
        });

        await DreamersMechanicsHelper.ExhaustAllTeamBlasts(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Request"].UpgradeValueBy(-1M);
    }
}
