using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Ups initiated by Shion (see <see cref="TeamUpCard"/>).
/// </summary>
[Pool(typeof(ShionTheBruiserCardPool))]
public abstract class ShionTeamUpCard(int cost, CardType type, TargetType target) : TeamUpCard(cost, type, target)
{
    public override CharacterModel InitiatorCharacter => ModelDb.Character<Character.ShionTheBruiser>();
}
