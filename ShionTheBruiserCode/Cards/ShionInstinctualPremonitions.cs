using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionInstinctualPremonitions() : ShionTheBruiserCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BlockBonusAgainstAttacks", InstinctualPremonitions.BlockBonusAgainstAttacks),
        new DynamicVar("ArtifactBonusAgainstDebuffs", InstinctualPremonitions.ArtifactBonusAgainstDebuffs),
        new DynamicVar("StatsBonusAgainstBuffs", InstinctualPremonitions.StatsBonusAgainstBuffs),
        new DynamicVar("VigorBonusAgainstBlock", InstinctualPremonitions.VigorBonusAgainstBlock),
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var card = this;
        await PowerCmd.Apply<InstinctualPremonitions>(
            choiceContext,
            card.Owner.Creature,
            1,
            card.Owner.Creature,
            card
        );
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}