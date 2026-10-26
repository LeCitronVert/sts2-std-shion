using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionFlowGuard() : ShionBerserkCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ShionCardKeywords.Defensive];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Guard>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Defensive),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(3, ValueProp.Move),
        new CalculationBaseVar(3),
        new CalculationExtraVar(1),
        new CalculatedVar("BlockCount").WithMultiplier((model, _) => ShionMechanicsHelper.IsBerserk(model.Owner.Creature, ignoreDefensive: true) ? 1 : 0),
        new PowerVar<Guard>(1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        if (ShionMechanicsHelper.IsBerserk(card.Owner.Creature, ignoreDefensive: true))
        {
            ShionMechanicsHelper.MarkBerserkUsed(card.Owner.Creature);
        }
        for (int i = 0; i < ((CalculatedVar) DynamicVars["BlockCount"]).Calculate(this.Owner.Creature); i++)
        {
            await CreatureCmd.GainBlock(
                card.Owner.Creature,
                card.DynamicVars.Block,
                play,
                true
            );
        }
        await PowerCmd.Apply<Guard>(
            choiceContext,
            card.Owner.Creature,
            card.DynamicVars["Guard"].BaseValue,
            card.Owner.Creature,
            card
        );
        
        await ApplyDefensiveToPlayer();
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(1);
    }
    
    // ShionDefensiveCard stuff
    protected async Task ApplyDefensiveToPlayer()
    {
        await PowerCmd.Apply<DefensiveDebuff>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            amount: 1,
            applier: Owner.Creature,
            cardSource: this
        );
    }

    // Defensive cards aren't playable in a turn where a Berserk or Galvanised effect triggered
    protected override bool IsPlayable => !ShionMechanicsHelper.SpecialEffectTriggeredThisTurn(Owner.Creature);

    // Its own Berserk effect still applies while Defensive
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsBerserk(Owner.Creature, ignoreDefensive: true);
}