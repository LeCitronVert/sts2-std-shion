using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using MegaCrit.Sts2.Core.Models.Cards;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Spell Blade (Team-Up), on Fry: whenever a Partner Forges, Forge half of that amount (times Amount).
/// Your Sovereign Blade costs Amount less, and adds an Ethereal copy of itself into each Partner's Hand when played.
/// </summary>
public class SpellBlade : TeamUpPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<SovereignBlade>()];

    public override async Task AfterForge(decimal amount, Player forger, AbstractModel? source)
    {
        if (amount <= 0 || Owner.IsDead || Owner.Player is not { } player || !LivingPartners.Contains(forger.Creature)) return;

        var half = Math.Floor(amount / 2) * Amount;
        if (half <= 0) return;

        Flash();
        await ForgeCmd.Forge(half, player, this);
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card is not SovereignBlade || card.Owner != Owner.Player || originalCost <= 0) return false;
        modifiedCost = Math.Max(0, originalCost - Amount);
        return true;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not SovereignBlade blade || blade.Owner != Owner.Player) return;
        if (cardPlay.PlayIndex < cardPlay.PlayCount - 1) return;

        foreach (var partner in LivingPartners.ToList())
        {
            if (partner.Player is not { } partnerPlayer) continue;

            Flash();
            var copy = SlalMechanicsHelper.CreateCopyForPlayer(blade, partnerPlayer);
            if (!copy.Keywords.Contains(CardKeyword.Ethereal)) copy.AddKeyword(CardKeyword.Ethereal);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner.Player);
        }
    }
}
