using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Cards;

/// <summary>
/// Team-Ups initiated by Seels (see <see cref="TeamUpCard"/>).
/// </summary>
[Pool(typeof(SeelsTheRecklessCardPool))]
public abstract class SeelsTeamUpCard(int cost, CardType type, TargetType target) : TeamUpCard(cost, type, target)
{
    public override CharacterModel InitiatorCharacter => ModelDb.Character<SeelsTheReckless>();
}
