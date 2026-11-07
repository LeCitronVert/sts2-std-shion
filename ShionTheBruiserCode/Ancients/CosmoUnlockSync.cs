using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Unlocks;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Ancients;

/// <summary>
/// Local-profile data that Cosmo needs to read for every player of a run, carried inside each player's unlock state.
/// The lobby already sends every player's unlock state to everyone (that's how vanilla shares epochs), so this
/// rides along with it: 7 bits for the unlocked Chaos Emeralds, 1 bit for "Cosmo next" (only the host's is read).
/// Every client then makes the same Cosmo decisions without any extra message.
/// Not kept in the run save: after loading a run the extras are empty, the same on every client.
/// </summary>
public sealed class CosmoUnlockExtras
{
    public byte EmeraldMask { get; init; }
    public bool CosmoNext { get; init; }

    private const int Bits = 8;
    private const int CosmoNextBit = 1 << 7;

    public bool HasEmerald(ChaosEmeraldColor color) => (EmeraldMask & (1 << (int)color)) != 0;

    public int Pack() => EmeraldMask | (CosmoNext ? CosmoNextBit : 0);

    public static CosmoUnlockExtras Unpack(int packed) => new()
    {
        EmeraldMask = (byte)(packed & 0x7F),
        CosmoNext = (packed & CosmoNextBit) != 0,
    };

    public static CosmoUnlockExtras FromLocalProfile() => new()
    {
        EmeraldMask = (byte)ChaosEmeraldHelper.UnlockedColors.Aggregate(0, (mask, color) => mask | 1 << (int)color),
        CosmoNext = CosmoSaveData.MultiplayerCosmoNext,
    };

    private static readonly CosmoUnlockExtras Empty = new();
    private static readonly ConditionalWeakTable<object, CosmoUnlockExtras> Table = new();

    public static CosmoUnlockExtras Of(UnlockState state) => Table.TryGetValue(state, out var extras) ? extras : Empty;

    public static CosmoUnlockExtras Of(Player player) => Of(player.UnlockState);

    internal static void Attach(object state, CosmoUnlockExtras extras) => Table.AddOrUpdate(state, extras);

    internal static CosmoUnlockExtras? Find(object state) => Table.TryGetValue(state, out var extras) ? extras : null;

    internal static void Write(PacketWriter writer, CosmoUnlockExtras? extras) => writer.WriteInt(extras?.Pack() ?? 0, Bits);

    internal static CosmoUnlockExtras Read(PacketReader reader) => Unpack(reader.ReadInt(Bits));
}

/// <summary>Every local unlock state (solo player, lobby host, joining client) is built from the progress save.</summary>
[HarmonyPatch(typeof(UnlockState), MethodType.Constructor, [typeof(ProgressState)])]
public static class CosmoUnlockStateFromProgressPatch
{
    [HarmonyPostfix]
    public static void Postfix(UnlockState __instance)
    {
        try
        {
            CosmoUnlockExtras.Attach(__instance, CosmoUnlockExtras.FromLocalProfile());
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"Could not read Cosmo unlock data: {ex}");
        }
    }
}

[HarmonyPatch(typeof(UnlockState), nameof(UnlockState.ToSerializable))]
public static class CosmoUnlockStateToSerializablePatch
{
    [HarmonyPostfix]
    public static void Postfix(UnlockState __instance, SerializableUnlockState __result)
    {
        if (CosmoUnlockExtras.Find(__instance) is { } extras) CosmoUnlockExtras.Attach(__result, extras);
    }
}

[HarmonyPatch(typeof(UnlockState), nameof(UnlockState.FromSerializable))]
public static class CosmoUnlockStateFromSerializablePatch
{
    [HarmonyPostfix]
    public static void Postfix(SerializableUnlockState unlockState, UnlockState __result)
    {
        if (unlockState != null && CosmoUnlockExtras.Find(unlockState) is { } extras) CosmoUnlockExtras.Attach(__result, extras);
    }
}

[HarmonyPatch(typeof(SerializableUnlockState), nameof(SerializableUnlockState.Serialize))]
public static class CosmoUnlockStateSerializePatch
{
    [HarmonyPostfix]
    public static void Postfix(SerializableUnlockState __instance, PacketWriter writer) =>
        CosmoUnlockExtras.Write(writer, CosmoUnlockExtras.Find(__instance));
}

[HarmonyPatch(typeof(SerializableUnlockState), nameof(SerializableUnlockState.Deserialize))]
public static class CosmoUnlockStateDeserializePatch
{
    [HarmonyPostfix]
    public static void Postfix(SerializableUnlockState __instance, PacketReader reader) =>
        CosmoUnlockExtras.Attach(__instance, CosmoUnlockExtras.Read(reader));
}
