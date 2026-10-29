using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using static ShionTheBruiser.ShionTheBruiserCode.Ancients.PartyDialogue;
using static ShionTheBruiser.ShionTheBruiserCode.Ancients.PartyDialogues;

namespace ShionTheBruiser.ShionTheBruiserCode.Ancients;

/// <summary>
/// Shion's Neow party interactions (Slay the Dreamers interactions sheet). Text is in ancients.json.
/// Speakers: Self = Shion, 0/1/2 = the group's matched partners in order.
/// </summary>
public static class ShionPartyDialogues
{
    private static PartyDialogue Once(params int[] speakers) => new(false, speakers);
    private static PartyDialogue Repeat(params int[] speakers) => new(true, speakers);

    public static IReadOnlyList<PartyDialogueGroup> Groups =>
    [
        new("IRONCLAD", All(ModelDb.Character<Ironclad>),
            Once(Self, 0, Self),
            Once(Self, Self, Self),
            Repeat(Self),
            Once(Self, 0, Self, Self, Self, Self),
            Repeat(Self),
            Once(Self, 0, Self),
            Repeat(Self)),

        new("SILENT", All(ModelDb.Character<Silent>),
            Once(Self, 0, Self),
            Repeat(Self),
            Once(Self, 0, Self, Self, Self, 0, Self),
            Repeat(Self),
            Once(Self, Self, 0, Self, Self, 0),
            Repeat(Self),
            Once(Self, Self, Self, Self, 0, 0, Self, 0),
            Repeat(Self)),

        new("DEFECT", All(ModelDb.Character<Defect>),
            Once(Self, 0, Self),
            Once(0, Self, 0, Self, 0),
            Repeat(Self),
            Once(0, Self, 0, Self),
            Repeat(Self),
            Once(0, Self, Self, 0, Self, Self),
            Repeat(Self)),

        new("STS1", All(ModelDb.Character<Ironclad>, ModelDb.Character<Silent>, ModelDb.Character<Defect>),
            Once(Self, 0, 1, 2, Self),
            Repeat(Self)),

        new("NECROBINDER", All(ModelDb.Character<Necrobinder>),
            Once(0, Self, 0, Self),
            Once(Self, 0, Self, Self, Self),
            Repeat(0),
            Once(0, Self, 0, Self),
            Repeat(Self),
            Once(0, Self, 0, Self)),

        new("REGENT", All(ModelDb.Character<Regent>),
            Once(0, Self, 0, Self),
            Repeat(Self),
            Once(Self, 0, Self, 0),
            Repeat(Self),
            Once(0, Self, 0),
            Repeat(Self)),

        new("STS2", All(ModelDb.Character<Necrobinder>, ModelDb.Character<Regent>),
            Once(Self, 0, 1, Self),
            Repeat(Self)),

        new("DREAMERS", AnyOf(ModelDb.Character<SlalTheEndless>, ModelDb.Character<FryTheCommander>, ModelDb.Character<SeelsTheReckless>),
            Repeat(Self),
            Repeat(Self),
            Repeat(Self)),

        new("MODDED", OtherModded(),
            Repeat(Self),
            Repeat(Self),
            Repeat(Self)),

        new("SHION", SameCharacter(),
            Once(Self, 0, Self),
            Repeat(0),
            Repeat(Self)),
    ];
}
