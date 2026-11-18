using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Loader;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace ShionTheBruiser.Loader;

/// <summary>
/// Shipped as ShionTheBruiser.dll (the DLL the game loads for the mod). Picks the build matching the running game
/// branch (ShionTheBruiser.Live.dll / ShionTheBruiser.Beta.dll next to it), loads it into the game's context,
/// registers it with the mod so its types get mod/multiplayer ids, and runs its [ModInitializer]s.
/// </summary>
[ModInitializer(nameof(Initialize))]
public static class BranchLoader
{
    private const string ModId = "ShionTheBruiser";
    /// <summary>Type that only exists on the beta; used when the game has no release info to read a version from.</summary>
    private const string BetaOnlyType = "MegaCrit.Sts2.Core.Models.Characters.Fool";

    private static readonly Logger Logger = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        var self = typeof(BranchLoader).Assembly;
        var dir = Path.GetDirectoryName(self.Location)
                  ?? throw new InvalidOperationException($"Can't locate the {ModId} mod folder (loader location '{self.Location}')");

        var (beta, why) = DetectBeta();
        var branch = beta ? "Beta" : "Live";
        var path = Path.Combine(dir, $"{ModId}.{branch}.dll");
        Logger.Info($"Game branch: {branch} ({why}); loading {path}");
        // Loading the other branch's build would only fail later on missing methods, so refuse outright.
        if (!File.Exists(path))
            throw new FileNotFoundException($"{ModId}: no build for the {branch} branch at {path}", path);

        var context = AssemblyLoadContext.GetLoadContext(self) ?? AssemblyLoadContext.Default;
        var assembly = context.LoadFromAssemblyPath(path);
        ModManager.AssociateAssemblyWithMod(ModId, assembly);

        var initializers = assembly.GetTypes().Where(t => t.GetCustomAttribute<ModInitializerAttribute>() != null).ToList();
        if (initializers.Count == 0)
            throw new InvalidOperationException($"{ModId}: {path} has no [ModInitializer]");
        foreach (var type in initializers)
        {
            var name = type.GetCustomAttribute<ModInitializerAttribute>()!.initializerMethod;
            var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                         ?? throw new MissingMethodException(type.FullName, name);
            Logger.Info($"Calling {type.FullName}.{name}");
            try
            {
                method.Invoke(null, null);
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }
        }
    }

    private static (bool beta, string why) DetectBeta()
    {
        var min = ParseVersion(typeof(BranchLoader).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(a => a.Key == "Sts2BetaMinVersion").Value!);

        if (ReleaseInfoManager.Instance.SemVer is { } v)
        {
            // Compare the numbers only; SemanticVersion would rank a prerelease below its release.
            var beta = (v.Major, v.Minor, v.Patch).CompareTo(min) >= 0;
            return (beta, $"game v{v.Major}.{v.Minor}.{v.Patch}, beta from v{min.Item1}.{min.Item2}.{min.Item3}");
        }

        var hasBetaType = typeof(ModManager).Assembly.GetType(BetaOnlyType) != null;
        return (hasBetaType, $"no release info; {BetaOnlyType} {(hasBetaType ? "present" : "absent")}");
    }

    private static (int, int, int) ParseVersion(string s)
    {
        var parts = s.Split('.').Select(int.Parse).ToArray();
        return (parts[0], parts[1], parts[2]);
    }
}
