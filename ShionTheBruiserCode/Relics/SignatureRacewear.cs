using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

public class SignatureRacewear() : ShionTheBruiserRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public const int BlockAmount = 4;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(BlockAmount, ValueProp.Unpowered)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ShionCardKeywords.NonNative)
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var relic = this;
        
        if (card.Owner != relic.Owner || ShionMechanicsHelper.IsCardNativeToCharacter(card, relic.Owner.Character)) 
            return;

        relic.Flash();
        await CreatureCmd.GainBlock(
            relic.Owner.Creature,
            DynamicVars.Block,
            null,
            true
        );
    }
}