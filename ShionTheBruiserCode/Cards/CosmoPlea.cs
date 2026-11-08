using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Cosmo's Plea (from Cosmo's Rulebook, any character). Gain 10 (14) Block.
/// Put ALL Attacks in your Hand on top of your Draw Pile. Exhaust. (Upgrade: Retain.)
/// You think "Maybe violence is not the issue here." (a thought rather than a line, so the Silent stays silent).
/// </summary>
[Pool(typeof(EventCardPool))]
public class CosmoPlea() : ShionTheBruiserCard(1,
    CardType.Skill, CardRarity.Ancient,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(10M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ThinkCmd.Play(new LocString("combat_messages", "SHIONTHEBRUISER-COSMO_PLEA.thought"), Owner.Creature);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        var attacks = PileType.Hand.GetPile(Owner).Cards.Where(c => c.Type == CardType.Attack).ToList();
        if (attacks.Count == 0) return;
        await CardPileCmd.Add(attacks, PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4M);
        AddKeyword(CardKeyword.Retain);
    }
}
