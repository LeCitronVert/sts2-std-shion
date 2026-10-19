using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionFlood() : ShionTheBruiserCard(1,
    CardType.Attack, CardRarity.Basic,
    TargetType.AnyEnemy), ITranscendenceCard
{
    public const int GalvanisedThreshold = 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1M, ValueProp.Move),
        new CalculationBaseVar(3),
        new CalculationExtraVar(1),
        new CalculatedVar("HitAmount").WithMultiplier((model, _) => ShionMechanicsHelper.IsGalvanised(model.Owner.Creature, GalvanisedThreshold) ? 1 : 0)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Galvanised),
    ];
    
    protected override bool ShouldGlowGoldInternal => ShionMechanicsHelper.IsGalvanised(this.Owner.Creature, GalvanisedThreshold);

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionFlood card = this;
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .WithHitCount((int) ((CalculatedVar) card.DynamicVars["HitAmount"]).Calculate(card.Owner.Creature))
            .FromCard(card)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext)
        ;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(1);
    }

    public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<ShionBreakDown>();
}