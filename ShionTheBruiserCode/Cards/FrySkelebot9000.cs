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
using ShionTheBruiser.ShionTheBruiserCode.Monsters;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Up (Necrobinder), Fry: you and your partner summon a Corrupted Punch Construct with 20 HP.
/// Your partner's Summons add Blocking intents to both Constructs equal to the amount Summoned, and spending Stars adds Attacking intents equal to the Stars spent. Exhaust. (Retain.)
/// </summary>
public class FrySkelebot9000() : FryTeamUpCard(3,
    CardType.Skill,
    TargetType.Self)
{
    public const int ConstructHp = 20;

    public override CharacterModel PartnerCharacter => ModelDb.Character<Necrobinder>();

    protected override IEnumerable<CardKeyword> TeamUpKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("ConstructHp", ConstructHp),
        new DamageVar(CorruptedPunchConstructPower.DamagePerAttack, ValueProp.Move),
        new BlockVar(CorruptedPunchConstructPower.BlockPerBlock, ValueProp.Unpowered),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<CorruptedPunchConstructPower>()];

    protected override async Task OnTeamUp(PlayerChoiceContext choiceContext, CardPlay play, Player initiator, Player partner)
    {
        await CorruptedPunchConstruct.Summon(choiceContext, initiator, DynamicVars["ConstructHp"].BaseValue, this);
        await CorruptedPunchConstruct.Summon(choiceContext, partner, DynamicVars["ConstructHp"].BaseValue, this);
        await TeamUpPower.Apply<Skelebot9000>(choiceContext, initiator.Creature, partner.Creature, 1, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
