using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using ShionTheBruiser.ShionTheBruiserCode.Character;
using static ShionTheBruiser.ShionTheBruiserCode.Ancients.PartyDialogue;
using static ShionTheBruiser.ShionTheBruiserCode.Ancients.PartyDialogues;

namespace ShionTheBruiser.ShionTheBruiserCode.Ancients;

/// <summary>
/// Seels' Neow party interactions (Slay the Dreamers interactions sheet). Text is in ancients.json.
/// Speakers: Self = Seels, 0/1/2 = the group's matched partners in order.
/// </summary>
public static class SeelsPartyDialogues
{
    private static PartyDialogue Once(params int[] speakers) => new(false, speakers);
    private static PartyDialogue Repeat(params int[] speakers) => new(true, speakers);

    public static IReadOnlyList<PartyDialogueGroup> Groups =>
    [
        new("IRONCLAD", All(ModelDb.Character<Ironclad>),
            Once(Self, 0, Self),
            Repeat(Self),
            Repeat(Self)),

        new("SILENT", All(ModelDb.Character<Silent>),
            Once(Self, 0, Self),
            Repeat(Self),
            Repeat(0)),

        new("DEFECT", All(ModelDb.Character<Defect>),
            Once(Self, 0, Self, 0),
            Repeat(Self),
            Repeat(0)),

        new("STS1", All(ModelDb.Character<Ironclad>, ModelDb.Character<Silent>, ModelDb.Character<Defect>),
            Repeat(Self)),

        new("REGENT", All(ModelDb.Character<Regent>),
            Once(0, Self, 0, Self),
            Repeat(Self),
            Repeat(0)),

        new("NECROBINDER", All(ModelDb.Character<Necrobinder>),
            Once(Self, 0, Self, 0),
            Repeat(Self),
            Repeat(0),
            Repeat(Self),
            Once(Self, 0, Self, 0, Self),
            Repeat(Self),
            Repeat(0)),

        // 0 = Regent, 1 = Necrobinder.
        new("STS2", All(ModelDb.Character<Regent>, ModelDb.Character<Necrobinder>),
            Once(0, 1, Self, 0, Self),
            Repeat(Self)),

        new("DREAMERS", AnyOf(ModelDb.Character<SlalTheEndless>, ModelDb.Character<FryTheCommander>, ModelDb.Character<Character.ShionTheBruiser>),
            Repeat(Self),
            Repeat(Self),
            Repeat(Self)),

        new("SEELS", SameCharacter(),
            Once(Self, 0, Self, 0),
            Repeat(0),
            Repeat(Self)),

        new("SHION", All(ModelDb.Character<Character.ShionTheBruiser>),
            Once(0, Self, 0, Self),
            Repeat(0),
            Repeat(Self)),
    ];
}
