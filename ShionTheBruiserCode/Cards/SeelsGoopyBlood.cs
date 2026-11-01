using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Slal): when your partner creates a Slimed, gain 1 Strength. When you create a Wound, your partner gains 2 Random.
/// </summary>
public class SeelsGoopyBlood() : SeelsTeamUpCard(2,
    CardType.Power,
    TargetType.Self)
{
    public override CharacterModel PartnerCharacter => ModelDb.Character<SlalTheEndless>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(GoopyBlood.StrengthPerSlimed),
        new DynamicVar("Random", GoopyBlood.RandomPerWound),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<Slimed>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromCard<Wound>(),
        HoverTipFactory.FromPower<Powers.Random>(),
    ];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        await TeamUpPower.Apply<GoopyBlood>(choiceContext, initiator.Creature, partner.Creature, 1, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
