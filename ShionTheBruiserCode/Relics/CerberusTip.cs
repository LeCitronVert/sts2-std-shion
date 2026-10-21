using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Relics;

/// <summary>
/// The first time you Blood Bend X each combat, heal X HP.
/// </summary>
public class CerberusTip : SeelsTheRecklessRelic, ISeelsListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(SeelsCardKeywords.BloodBend)];

    private bool _usedThisCombat;

    public override Task BeforeCombatStart()
    {
        _usedThisCombat = false;
        Status = RelicStatus.Active;
        return Task.CompletedTask;
    }

    public async Task AfterBloodBend(PlayerChoiceContext choiceContext, Player player, int x, CardModel? source)
    {
        if (player != Owner || _usedThisCombat) return;

        _usedThisCombat = true;
        Status = RelicStatus.Normal;
        if (x <= 0) return;

        Flash();
        await CreatureCmd.Heal(Owner.Creature, x);
    }
}
