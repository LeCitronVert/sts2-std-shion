using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

public static class MonsterPowersHelper
{
    private record Applicator(Type AppliedType, Func<PlayerChoiceContext, Creature, int, Creature, CardModel, Task> Apply);

    private static Applicator As<T>() where T : PowerModel =>
        new(typeof(T), (ctx, target, amount, applier, card) => PowerCmd.Apply<T>(ctx, target, amount, applier, card));

    // One entry per power you want Monstermancy to be able to copy, mapped to the power the copy actually applies.
    // Anything not in here is implicitly "can't be copied that way."
    private static readonly Dictionary<Type, Applicator> _applicators = new()
    {
        [typeof(WeakPower)] = As<WeakPower>(),
        [typeof(FrailPower)] = As<VulnerablePower>(),
        [typeof(VulnerablePower)] = As<VulnerablePower>(),
        [typeof(StrengthPower)] = As<StrengthPower>(),
        [typeof(DexterityPower)] = As<DexterityPower>(),
        [typeof(VigorPower)] = As<VigorPower>(),
        [typeof(RitualPower)] = As<RitualPower>(),
        [typeof(RegenPower)] = As<RegenPower>(),
        [typeof(ArtifactPower)] = As<ArtifactPower>(),
        [typeof(IntangiblePower)] = As<IntangiblePower>(),
        [typeof(ThornsPower)] = As<ThornsPower>(),
        [typeof(BufferPower)] = As<BufferPower>(),
        [typeof(PersonalHivePower)] = As<ThornsPower>(),
        [typeof(PlatingPower)] = As<PlatingPower>(),
        [typeof(SteamEruptionPower)] = As<VigorPower>(),
        [typeof(PlowPower)] = As<RitualPower>(),
        [typeof(SandpitPower)] = As<PlatingPower>(),
        [typeof(ChainsOfBindingPower)] = As<NeurosurgePower>(),
        [typeof(VitalSparkPower)] = As<RegenPower>(),
        [typeof(HexPower)] = As<NeurosurgePower>(),
        [typeof(DampenPower)] = As<NeurosurgePower>(),
        [typeof(BurrowedPower)] = As<BarricadePower>(),
        [typeof(HighVoltagePower)] = As<RitualPower>(),
    };

    public static bool IsCopyable(Type powerType) => _applicators.ContainsKey(powerType);

    /// <summary>
    /// The power a copied enemy power turns into when Monstermancy applies it (e.g. Frail becomes Vulnerable).
    /// </summary>
    public static PowerModel GetAppliedPower(Type powerType) =>
        ModelDb.GetById<PowerModel>(ModelDb.GetId(GetApplicator(powerType).AppliedType));

    public static Task Apply(
        PlayerChoiceContext choiceContext,
        Type powerType,
        Creature target,
        int amount,
        Creature applier,
        CardModel cardSource)
    {
        return GetApplicator(powerType).Apply(choiceContext, target, amount, applier, cardSource);
    }

    private static Applicator GetApplicator(Type powerType) =>
        _applicators.TryGetValue(powerType, out var applicator)
            ? applicator
            : throw new InvalidOperationException($"Power {powerType.Name} is not registered as copyable by Monstermancy.");
}
