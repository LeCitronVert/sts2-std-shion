using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

[Pool(typeof(TokenCardPool))]
public class No() : ShionTheBruiserCard(-1,
    CardType.Skill, CardRarity.Token,
    TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    public override bool CanBeGeneratedInCombat => false;
    public override int MaxUpgradeLevel => 0;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
    }

    protected override void OnUpgrade()
    {

    }
}