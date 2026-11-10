using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Strife;

/// <summary>
/// Granted to every player by the Chaos Emeralds at the end of Act 3 (<see cref="StrifeTransition"/>).
/// At the start of each fight, enter <see cref="SuperFormPower"/>. Your Gold is shown as Rings (cosmetic, see
/// <see cref="RingVisuals"/>).
/// </summary>
[Pool(typeof(EventRelicPool))]
public class SuperForm : ShionTheBruiserRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => "super_form.png".RelicImagePath();
    // No outline drawn yet (MISSING_IMAGES.tsv)
    protected override string PackedIconOutlinePath => "relic_outline.png".RelicImagePath();
    protected override string BigIconPath => PackedIconPath;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SuperFormPower>()];

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<SuperFormPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, null);
    }

    public override Task AfterObtained()
    {
        RingVisuals.RefreshGoldIcon();
        return Task.CompletedTask;
    }

    public static bool Has(Player? player) => player?.Relics.Any(r => r is SuperForm) == true;
}

/// <summary>
/// Unblocked damage you take is absorbed by your Rings (Gold) first, 1 Ring per point, until you run out.
/// Damage you deal yourself isn't absorbed.
/// Whenever an enemy dies, shuffle all your Emerald Powers into your Draw Pile, wherever they are, and if the Team Blast
/// gauge was used this combat, it's unlocked and back to 0 for the whole team.
/// Visuals (aura, sparks, sweat, Ring health bar segment, music): <see cref="SuperFormFx"/>, <see cref="RingVisuals"/>.
/// </summary>
public sealed class SuperFormPower : ShionTheBruiserPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // Same icon as the Super Form relic
    public override string CustomPackedIconPath => "super_form.png".RelicImagePath();
    public override string CustomBigIconPath => "super_form.png".RelicImagePath();

    private int _pendingRingLoss;

    private int Rings => Owner.Player?.Gold ?? 0;

    // Only called by real damage (not by previews), so tracking the absorbed amount here is safe
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || amount <= 0 || dealer == Owner || Owner.Player == null) return amount;
        var available = Rings - _pendingRingLoss;
        if (available <= 0) return amount;

        var absorbed = (int)Math.Min(available, Math.Ceiling(amount));
        _pendingRingLoss += absorbed;
        return Math.Max(0m, amount - absorbed);
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        if (_pendingRingLoss <= 0 || Owner.Player is not { } player) return;
        var loss = _pendingRingLoss;
        _pendingRingLoss = 0;
        Flash();
        await PlayerCmd.LoseGold(loss, player, GoldLossType.Lost);
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature.Side == Owner.Side || Owner.IsDead || Owner.Player is not { } player) return;

        // A used Team Blast gauge is unlocked and back to 0 for the whole team
        if (Singletons.TeamBlastSingleton.Instance?.ResetUsedGauge() == true) Flash();

        // Those already in the Draw Pile stay where they are
        var emeraldPowers = new[] { PileType.Hand, PileType.Discard, PileType.Exhaust }
            .SelectMany(pile => pile.GetPile(player).Cards)
            .Where(c => c.Keywords.Contains(DreamersKeywords.EmeraldPower))
            .ToList();
        if (emeraldPowers.Count == 0) return;

        Flash();
        foreach (var card in emeraldPowers)
        {
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random);
        }
    }
}
