using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Variables;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Cooldown 4 (3). Gain 1 Dexterity for each type of card you played this turn (Attack, Skill, Power, Status, Curse). (Silent)
/// Token: one of Anything You Can Do's options (<see cref="FryAnythingYouCanDo"/>).
/// </summary>
[Pool(typeof(TokenCardPool))]
public class FryDiagonalSpeedBoost() : FryCooldownCard(1,
    CardType.Skill, CardRarity.Token,
    TargetType.Self)
{
    protected override int BaseCooldown => 4;

    protected override IEnumerable<DynamicVar> CooldownCardVars => [
        new PowerVar<DexterityPower>(1M),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("TypesAmount").WithMultiplier((card, _) => TypesPlayedThisTurn(card)),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];

    private static readonly CardType[] Types = [CardType.Attack, CardType.Skill, CardType.Power, CardType.Status, CardType.Curse];

    // This card's own play counts too (it's a Skill)
    private static int TypesPlayedThisTurn(CardModel card)
    {
        if (card.CombatState is null) return 0;
        var played = CombatManager.Instance.History.CardPlaysStarted
            .Where(e => e.CardPlay.Card.Owner == card.Owner && e.HappenedThisTurn(card.CombatState))
            .Select(e => e.CardPlay.Card.Type)
            .Append(card.Type)
            .ToHashSet();
        return Types.Count(played.Contains);
    }

    protected override async Task OnCooldownCardPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var types = TypesPlayedThisTurn(this);
        if (types <= 0) return;
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars.Dexterity.BaseValue * types, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[CooldownKey].UpgradeValueBy(-1M);
    }
}
