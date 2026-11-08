using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Ancients;

/// <summary>
/// Cosmo, Mother of (re)Animation. Unlocked by the "Warm Embrace" epoch, she sometimes welcomes the party instead of
/// Neow (<see cref="CosmoSpawn"/>); she never shows up in Acts 2 and 3.
/// Like Neow, she brings the player back to full HP.
///
/// Options, from the sheet: one multiplayer-only relic when possible, one cursed relic (unless the multiplayer one
/// already is), and non-cursed compatible relics for the rest. Each relic declares its rules (<see cref="ICosmoOption"/>).
///
/// Dialogue: see <see cref="CosmoDialogues"/>.
/// </summary>
public class Cosmo : CustomAncientModel
{
    public const int OptionCount = 3;

    private const string CursedDonePage = "SHIONTHEBRUISER-COSMO.pages.DONE.CURSED.description";

    /// <summary>Every relic she can offer, in the sheet's order.</summary>
    public static IReadOnlyList<RelicModel> AllOptions =>
    [
        ModelDb.Relic<PowerCore>(),
        ModelDb.Relic<FriendshipBracelet>(),
        ModelDb.Relic<MasterPendant>(),
        ModelDb.Relic<SailingShip>(),
        ModelDb.Relic<Checkpoint>(),
        ModelDb.Relic<ComfyCape>(),
        ModelDb.Relic<SuperBulldozuratorPuzzlitowersonic>(),
        ModelDb.Relic<PeacePromise>(),
        ModelDb.Relic<ChaosRing>(),
        ModelDb.Relic<LeafRobe>(),
        ModelDb.Relic<InvisibleNotebook>(),
        ModelDb.Relic<ForceCursor>(),
        ModelDb.Relic<ImmortalRose>(),
        ModelDb.Relic<LuckyGlove>(),
        ModelDb.Relic<LemonadeBottles>(),
        ModelDb.Relic<PopCorn>(),
        ModelDb.Relic<PoemOfGratitude>(),
        ModelDb.Relic<PlanetEgg>(),
        ModelDb.Relic<FestiveRibbons>(),
        ModelDb.Relic<SouvenirSeed>(),
        ModelDb.Relic<WateringCan>(),
        ModelDb.Relic<SantaHat>(),
        ModelDb.Relic<FlosFridge>(),
        ModelDb.Relic<SecretKey>(),
        ModelDb.Relic<CosmosRulebook>(),
    ];

    // Only used for AllPossibleOptions (Relic Collection, "ancient" console command): the real roll is GenerateInitialOptions.
    protected override OptionPools MakeOptionPools => new(MakePool(AllOptions.ToArray()));

    // She only ever replaces Neow, never a random Act 2/3 ancient
    public override bool IsValidForAct(ActModel act) => false;

    public override string? CustomScenePath => Path.Join(MainFile.ResPath, "scenes", "ancients", "cosmo.tscn");
    public override string? CustomMapIconPath => "ancients/cosmo_map.png".ImagePath();
    public override string? CustomMapIconOutlinePath => "ancients/cosmo_map_outline.png".ImagePath();
    public override string? CustomRunHistoryIconPath => "ancients/cosmo_icon.png".ImagePath();
    public override string? CustomRunHistoryIconOutlinePath => "ancients/cosmo_icon_outline.png".ImagePath();

    // Leafy green bubble, warm dark buttons
    public override Color DialogueColor => new("3f6b2c");
    public override Color ButtonColor => new(0.22f, 0.12f, 0.04f, 0.5f);

    /// <summary>Line 0 of the first-visit-ever dialogue onward, as many lines as the loc file has.</summary>
    protected override AncientDialogueSet DefineDialogues()
    {
        var set = base.DefineDialogues();
        var firstVisitLines = 0;
        while (LocString.Exists(LocTable, $"{Id.Entry}.talk.firstVisitEver.0-{firstVisitLines}.ancient")
               || LocString.Exists(LocTable, $"{Id.Entry}.talk.firstVisitEver.0-{firstVisitLines}.char"))
        {
            firstVisitLines++;
        }

        return new AncientDialogueSet
        {
            FirstVisitEverDialogue = firstVisitLines > 0 ? new AncientDialogue(Enumerable.Repeat("", firstVisitLines).ToArray()) : null,
            CharacterDialogues = set.CharacterDialogues,
            AgnosticDialogues = set.AgnosticDialogues,
        };
    }

    // Like Neow: the player is brought back from the brink, then healed to full
    protected override async Task BeforeEventStarted(bool isPreFinished)
    {
        if (!isPreFinished) Owner!.Creature.SetCurrentHpInternal(0m);
        await base.BeforeEventStarted(isPreFinished);
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        RollOptions()
            .Select(relic => RelicOption(relic.ToMutable(), "INITIAL", ((ICosmoOption)relic).IsCursedOption ? CursedDonePage : null))
            .ToList();

    private List<RelicModel> RollOptions()
    {
        var owner = Owner!;
        var multiplayer = owner.RunState.Players.Count > 1;

        var candidates = AllOptions
            .Where(r => r is ICosmoOption option
                        && option.CosmoAvailability switch
                        {
                            CosmoAvailability.SoloOnly => !multiplayer,
                            CosmoAvailability.MultiplayerOnly => multiplayer,
                            _ => true,
                        }
                        && option.IsAllowedAtCosmo(owner)
                        && owner.Relics.All(owned => owned.Id != r.Id))
            .ToList();

        RelicModel? multiplayerPick = null;
        RelicModel? cursedPick = null;
        List<RelicModel> picked = [];

        bool CanAdd(RelicModel relic) => !picked.Contains(relic) && picked.All(p => !AreIncompatible(p, relic));

        if (multiplayer)
        {
            multiplayerPick = Pick(candidates.Where(r => Option(r).CosmoAvailability == CosmoAvailability.MultiplayerOnly && CanAdd(r)));
            if (multiplayerPick != null) picked.Add(multiplayerPick);
        }

        if (multiplayerPick == null || !Option(multiplayerPick).IsCursedOption)
        {
            cursedPick = Pick(candidates.Where(r => Option(r).IsCursedOption && CanAdd(r)));
            if (cursedPick != null) picked.Add(cursedPick);
        }

        List<RelicModel> others = [];
        while (picked.Count < OptionCount)
        {
            var other = Pick(candidates.Where(r => !Option(r).IsCursedOption
                                                   && Option(r).CosmoAvailability != CosmoAvailability.MultiplayerOnly
                                                   && CanAdd(r)));
            if (other == null) break;
            picked.Add(other);
            others.Add(other);
        }

        // Like Neow: the regular gifts first, the cursed one last
        List<RelicModel> ordered = [.. others];
        if (multiplayerPick != null) ordered.Add(multiplayerPick);
        if (cursedPick != null) ordered.Add(cursedPick);
        return ordered;
    }

    private RelicModel? Pick(IEnumerable<RelicModel> pool)
    {
        var list = pool.ToList();
        return list.Count == 0 ? null : Rng.NextItem(list);
    }

    private static ICosmoOption Option(RelicModel relic) => (ICosmoOption)relic;

    private static bool AreIncompatible(RelicModel a, RelicModel b) =>
        Option(a).IncompatibleOptions.Contains(b.GetType()) || Option(b).IncompatibleOptions.Contains(a.GetType());
}
