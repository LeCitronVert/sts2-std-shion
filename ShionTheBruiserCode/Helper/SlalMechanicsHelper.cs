using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using Random = ShionTheBruiser.ShionTheBruiserCode.Powers.Random;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

public static class SlalMechanicsHelper
{
    public static Task TriggerNovel(CardModel card, Creature character)
    {
        card.RemoveKeyword(SlalCardKeywords.Novel);

        foreach (var copy in (character.Player?.PlayerCombatState?.AllCards ?? []).Where(c => c.Id == card.Id))
        {
            copy.RemoveKeyword(SlalCardKeywords.Novel);
        }

        PowerCmd.Apply<Random>(
            new ThrowingPlayerChoiceContext(),
            character,
            1,
            character,
            null
        );
        
        return Task.CompletedTask;
    }

    public static Task EnsureCreatureHasNovelTracker(Creature creature)
    {
        if (!creature.HasPower<NovelTracker>())
        {
            PowerCmd.Apply<NovelTracker>(
                new ThrowingPlayerChoiceContext(),
                creature,
                1,
                creature,
                null,
                true
            );
        }

        return Task.CompletedTask;
    }
}