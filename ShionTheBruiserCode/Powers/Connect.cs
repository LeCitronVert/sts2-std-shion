using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// CONNECT (Team-Up), on the Defect: whenever you Evoke an Orb, your Partner Channels a copy of it.
/// Whenever your Partner plays a Power, gain 1 Focus. Both are multiplied by Amount.
/// </summary>
public class Connect : TeamUpPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Evoke),
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromPower<FocusPower>(),
    ];

    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        if (Owner.IsDead || orb.Owner.Creature != Owner) return;

        var partners = LivingPartners.Where(p => p.Player != null).ToList();
        if (partners.Count == 0) return;

        Flash();
        foreach (var partner in partners)
        {
            for (var i = 0; i < Amount; i++)
            {
                await OrbCmd.Channel(choiceContext, ModelDb.GetById<OrbModel>(orb.Id).ToMutable(), partner.Player!);
            }
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        // The copy of CONNECT that created this power doesn't count.
        if (Owner.IsDead || card.Type != CardType.Power || card is DefectConnect || !Partners.Contains(card.Owner.Creature)) return;

        Flash();
        await PowerCmd.Apply<FocusPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
