using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Helper;

public static class MonsterPowersHelper
{
    // One entry per power you want Monstermancy to be able to copy.
    // Anything not in here is implicitly "can't be copied that way."
    private static readonly Dictionary<Type, Func<PlayerChoiceContext, Creature, int, Creature, CardModel, Task>> _applicators = new()
    {
        [typeof(WeakPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<WeakPower>(ctx, target, amount, applier, card),

        [typeof(VulnerablePower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<VulnerablePower>(ctx, target, amount, applier, card),

        [typeof(StrengthPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<StrengthPower>(ctx, target, amount, applier, card),
        
        [typeof(DexterityPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<DexterityPower>(ctx, target, amount, applier, card),
        
        [typeof(VigorPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<VigorPower>(ctx, target, amount, applier, card),
        
        [typeof(RitualPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<RitualPower>(ctx, target, amount, applier, card),
        
        [typeof(RegenPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<RegenPower>(ctx, target, amount, applier, card),
        
        [typeof(ArtifactPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<ArtifactPower>(ctx, target, amount, applier, card),
        
        [typeof(IntangiblePower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<IntangiblePower>(ctx, target, amount, applier, card),
        
        [typeof(ThornsPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<ThornsPower>(ctx, target, amount, applier, card),
        
        [typeof(BufferPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<BufferPower>(ctx, target, amount, applier, card),
        
        [typeof(PersonalHivePower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<ThornsPower>(ctx, target, amount, applier, card),
        
        [typeof(PlatingPower)] = (ctx, target, amount, applier, card) =>
            PowerCmd.Apply<PlatingPower>(ctx, target, amount, applier, card),

        
    };

    public static bool IsCopyable(Type powerType) => _applicators.ContainsKey(powerType);

    public static Task Apply(
        PlayerChoiceContext choiceContext,
        Type powerType,
        Creature target,
        int amount,
        Creature applier,
        CardModel cardSource)
    {
        return !_applicators.TryGetValue(powerType, out var apply) ? throw new InvalidOperationException($"Power {powerType.Name} is not registered as copyable by Monstermancy.") : apply(choiceContext, target, amount, applier, cardSource);
    }
}