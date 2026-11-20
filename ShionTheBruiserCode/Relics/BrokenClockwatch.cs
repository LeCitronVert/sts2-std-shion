using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// Gain 1 Vigor each time one of your Requested counters ticks down.
/// </summary>
public class BrokenClockwatch : FryTheCommanderRelic, IFryListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VigorPower>(1M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(FryCardKeywords.Request),
        HoverTipFactory.FromPower<VigorPower>(),
    ];

    public async Task AfterRequestCounterTicked(PlayerChoiceContext choiceContext, Requested request)
    {
        if (request.Owner != Owner.Creature || Owner.Creature.IsDead) return;

        Flash();
        // Ticks happen at turn start or after a card's damage resolved, never mid-attack, so VigorPower is safe here.
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, null);
    }
}
