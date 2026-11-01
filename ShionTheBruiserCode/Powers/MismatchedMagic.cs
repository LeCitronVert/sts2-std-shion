using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Powers;

/// <summary>
/// Mismatched Magic (Team-Up), on Seels: whenever your Partner spends Stars, convert 1 Blood into 2 HP (times Amount, as Blood allows).
/// Whenever you Blood Bend, your Partner converts 3 Stars into 10 Bad Faith (times Amount, as Stars allow).
/// </summary>
public class MismatchedMagic : TeamUpPower, ISeelsListener
{
    public const int BloodConverted = 1;
    public const int HealPerConversion = 2;
    public const int StarsConverted = 3;
    public const int BadFaithPerConversion = 10;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Blood", BloodConverted),
        new DynamicVar("Heal", HealPerConversion),
        new DynamicVar("Stars", StarsConverted),
        new DynamicVar("BadFaith", BadFaithPerConversion),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<Blood>(),
        HoverTipFactory.FromPower<BadFaith>(),
    ];

    public override async Task AfterStarsSpent(int amount, Player spender)
    {
        if (amount <= 0 || Owner.IsDead || !Partners.Contains(spender.Creature)) return;

        var conversions = Math.Min((int)Amount, SeelsMechanicsHelper.GetBlood(Owner) / BloodConverted);
        if (conversions <= 0) return;

        Flash();
        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.ModifyAmount(context, Owner.GetPower<Blood>()!, -BloodConverted * conversions, Owner, null);
        await CreatureCmd.Heal(Owner, HealPerConversion * conversions);
    }

    public async Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source)
    {
        if (player != Owner.Player) return;

        foreach (var partner in LivingPartners.ToList())
        {
            if (partner.Player is not { } partnerPlayer) continue;

            var conversions = Math.Min((int)Amount, partnerPlayer.PlayerCombatState!.Stars / StarsConverted);
            if (conversions <= 0) continue;

            Flash();
            await PlayerCmd.LoseStars(StarsConverted * conversions, partnerPlayer);
            await PowerCmd.Apply<BadFaith>(choiceContext, partner, BadFaithPerConversion * conversions, Owner, null);
        }
    }
}
