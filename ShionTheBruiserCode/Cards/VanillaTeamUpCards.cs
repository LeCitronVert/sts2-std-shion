using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

// Team-Ups initiated by the vanilla characters (see TeamUpCard). Each base puts the card in its class's pool.

[Pool(typeof(IroncladCardPool))]
public abstract class IroncladTeamUpCard(int cost, CardType type, TargetType target) : TeamUpCard(cost, type, target)
{
    public override CharacterModel InitiatorCharacter => ModelDb.Character<Ironclad>();
}

[Pool(typeof(SilentCardPool))]
public abstract class SilentTeamUpCard(int cost, CardType type, TargetType target) : TeamUpCard(cost, type, target)
{
    public override CharacterModel InitiatorCharacter => ModelDb.Character<Silent>();
}

[Pool(typeof(RegentCardPool))]
public abstract class RegentTeamUpCard(int cost, CardType type, TargetType target) : TeamUpCard(cost, type, target)
{
    public override CharacterModel InitiatorCharacter => ModelDb.Character<Regent>();
}

[Pool(typeof(NecrobinderCardPool))]
public abstract class NecrobinderTeamUpCard(int cost, CardType type, TargetType target) : TeamUpCard(cost, type, target)
{
    public override CharacterModel InitiatorCharacter => ModelDb.Character<Necrobinder>();
}

[Pool(typeof(DefectCardPool))]
public abstract class DefectTeamUpCard(int cost, CardType type, TargetType target) : TeamUpCard(cost, type, target)
{
    public override CharacterModel InitiatorCharacter => ModelDb.Character<Defect>();
}

#if STS2_BETA
/// <summary>The Fool only exists on the game's beta branch.</summary>
[Pool(typeof(FoolCardPool))]
public abstract class FoolTeamUpCard(int cost, CardType type, TargetType target) : TeamUpCard(cost, type, target)
{
    public override CharacterModel InitiatorCharacter => ModelDb.Character<Fool>();
}
#endif
