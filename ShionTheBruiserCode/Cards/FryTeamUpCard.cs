using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Ups initiated by Fry (see <see cref="TeamUpCard"/>).
/// </summary>
[Pool(typeof(FryTheCommanderCardPool))]
public abstract class FryTeamUpCard(int cost, CardType type, TargetType target) : TeamUpCard(cost, type, target)
{
    public override CharacterModel InitiatorCharacter => ModelDb.Character<FryTheCommander>();
}
