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
/// Apply 2 Weak. Exhaust. (Request 5. Add nothing into your Hand. Actually adds All for Nothing, because it's funny.)
/// </summary>
public class FryFakeTexting() : FryTheCommanderCard(0, //todo : choose cost
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public const int RequestTurns = 5;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(2M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);

        if (!IsUpgraded) return;
        await FryMechanicsHelper.Request(choiceContext, this, RequestTurns, async _ =>
        {
            if (CombatState is null) return;
            var nothing = CombatState.CreateCard<FryAllForNothing>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(nothing, PileType.Hand, Owner);
        });
    }

    protected override void OnUpgrade()
    {
        AddKeyword(FryCardKeywords.Request);
    }
}
