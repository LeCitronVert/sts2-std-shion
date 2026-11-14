using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (anyone), Ironclad: this turn, you and your partner's next Attack (upgraded: next 2 Attacks) is played an extra time. Exhaust.
/// Uses the game's One-Two Punch power on both players.
/// </summary>
public class IroncladDoubleDown() : IroncladTeamUpCard(1,
    CardType.Skill,
    TargetType.Self)
{
    public override CharacterModel? PartnerCharacter => null;

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Attacks", 1)];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        var amount = DynamicVars["Attacks"].BaseValue;
        foreach (var player in new[] { initiator, partner }.Where(p => p.Creature.IsAlive))
        {
            await PowerCmd.Apply<OneTwoPunchPower>(choiceContext, player.Creature, amount, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Attacks"].UpgradeValueBy(1);
    }
}
