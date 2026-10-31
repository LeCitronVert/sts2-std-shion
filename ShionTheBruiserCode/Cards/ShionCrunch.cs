using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Variables;
using MegaCrit.Sts2.Core.HoverTips;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionCrunch() : ShionTheBruiserCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public const int GalvanisedThreshold = 3;
    public const int DebilitateThreshold = 9;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("DebuffAmount", 1),
        new CardsVar(1),
        new CriticalDrawVar(10)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<DebilitatePower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsGalvanised(Owner.Creature, GalvanisedThreshold);
    protected override bool ShouldGlowRedInternal => !ShouldGlowGoldInternal;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        var owner = card.Owner.Creature;

        if (!ShionMechanicsHelper.TriggerGalvanised(owner, GalvanisedThreshold))
        {
            await CardCmd.Exhaust(choiceContext, card);
            await CardPileCmd.Draw(choiceContext, card.DynamicVars.Cards.IntValue, card.Owner);
            return;
        }
        
        ArgumentNullException.ThrowIfNull(play.Target);
        
        // Checked before applying anything, Galvanised 3 already passed
        var isDebilitating = ShionMechanicsHelper.IsGalvanised(owner, DebilitateThreshold);
        
        await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, card.DynamicVars["DebuffAmount"].BaseValue, owner, card);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, card.DynamicVars["DebuffAmount"].BaseValue, owner, card);
        
        if (isDebilitating)
        {
            await PowerCmd.Apply<DebilitatePower>(choiceContext, play.Target, card.DynamicVars["DebuffAmount"].BaseValue, owner, card);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DebuffAmount"].UpgradeValueBy(1);
    }
    
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await ShionMechanicsHelper.ApplyCriticalDraw(this, choiceContext, card);
    }
}
