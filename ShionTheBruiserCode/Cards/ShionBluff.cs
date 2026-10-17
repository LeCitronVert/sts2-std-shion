using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

public class ShionBluff() : ShionBerserkCard(0,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new PowerVar<StrengthPower>(1M),
        new PowerVar<VigorPower>(3)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromKeyword(ShionCardKeywords.Berserk),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ShionBluff source = this;
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        CardModel? card = (await CardSelectCmd.FromHand(choiceContext, source.Owner, prefs, null, source)).FirstOrDefault();
        if (card != null)
            await CardCmd.Exhaust(choiceContext, card);
        await CreatureCmd.TriggerAnim(source.Owner.Creature, "Cast", source.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<StrengthPower>(choiceContext, source.Owner.Creature, source.DynamicVars["StrengthPower"].BaseValue, source.Owner.Creature, source);
        if (ShionMechanicsHelper.IsBerserk(source.Owner.Creature))
        {
            await PowerCmd.Apply<VigorPower>(choiceContext, source.Owner.Creature, source.DynamicVars["VigorPower"].BaseValue, source.Owner.Creature, source);
        }
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars["StrengthPower"].UpgradeValueBy(1M);
        DynamicVars["VigorPower"].UpgradeValueBy(2);
    }
}