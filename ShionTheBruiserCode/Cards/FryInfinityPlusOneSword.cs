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
/// Can only be played if you have 9 or more Attacks in your Discard Pile. Deal 52 (63) damage twice.
/// </summary>
public class FryInfinityPlusOneSword() : FryTheCommanderCard(0,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public const int RequiredAttacks = 9;

    public override int CanonicalStarCost => 9;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(52M, ValueProp.Move),
        new RepeatVar(2),
    ];

    protected override bool IsPlayable =>
        PileType.Discard.GetPile(Owner).Cards.Count(c => c.Type == CardType.Attack) >= RequiredAttacks;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(11M);
    }
}
