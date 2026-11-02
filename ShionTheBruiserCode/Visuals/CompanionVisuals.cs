using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Relics;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Shion's companions, shown next to whoever owns their relic:
/// <list type="bullet">
/// <item>Abby (<see cref="AbbyTheAbsol"/>): just in front of Shion at the merchant, by her side at rest sites.
/// In combat she's already a pet.</item>
/// <item>Edge (<see cref="EdgeTheChao"/>, scenes/companions/edge_chao.tscn): floats at Shion's top left in combat
/// ("!" when his relic procs), looks away from her with a "?" at the merchant, sits on the log with a heart at rest sites.</item>
/// </list>
/// Merchant and rest site positions are tuned on Shion's scenes (shion_merchant.tscn, shion_rest.tscn).
/// Visual state only: nothing here touches the game state.
/// </summary>
public static class CompanionVisuals
{
    /// <summary>Matches edge_chao.gd's Mode.</summary>
    private enum EdgeMode { Combat, Merchant, Rest }

    private static readonly string EdgeScenePath = Path.Join(MainFile.ResPath, "scenes", "companions", "edge_chao.tscn");

    private static string ImagePath(string name) => Path.Join(MainFile.ResPath, "images", "companions", name + ".png");

    /// <summary>Combat: Edge's center, from the top left corner of the creature's bounds.</summary>
    private static readonly Vector2 EdgeCombatOffset = new(-30f, 40f);

    // Merchant, in the merchant character's space (Shion's sprite is centered on (110.61, -183.29), feet at y = 0)
    private static readonly Vector2 EdgeMerchantPosition = new(-30f, -348f);
    private static readonly Vector2 AbbyMerchantPosition = new(176f, -60f);

    // Rest site, in ControlRoot's space (Shion's sprite is centered on (80, 100)).
    // Edge: a bit left of and lower than his usual spot at her top left.
    private static readonly Vector2 EdgeRestPosition = new(-225f, -40f);
    private static readonly Vector2 AbbyRestPosition = new(-56f, 180f);

    /// <summary>Raised when Edge the Chao's relic procs, to show his "!" (visual only).</summary>
    public static event Action<Creature>? EdgeProcced;

    public static void PlayEdgeProc(Creature creature) => EdgeProcced?.Invoke(creature);

    private static bool Has<T>(Player player) where T : RelicModel =>
        player.Relics.Any(r => r.Id == ModelDb.Relic<T>().Id);

    private static Node2D CreateEdge(EdgeMode mode)
    {
        var edge = GD.Load<PackedScene>(EdgeScenePath).Instantiate<Node2D>();
        edge.Call("set_mode", (int)mode);
        return edge;
    }

    private static Sprite2D CreateAbby(string image) => new()
    {
        Name = "AbbyCompanion",
        Texture = GD.Load<Texture2D>(ImagePath(image)),
    };

    /// <summary>Called by <see cref="CreatureFxController.Attach"/> once the creature's visuals are ready.</summary>
    public static void AttachToCombat(NCreature node)
    {
        var creature = node.Entity;
        var visuals = node.Visuals;
        if (creature.Player is not { } player || !Has<EdgeTheChao>(player)) return;

        var edge = CreateEdge(EdgeMode.Combat);
        visuals.AddChild(edge);
        edge.Position = visuals.Bounds.Position + EdgeCombatOffset;

        void OnProc(Creature c)
        {
            if (c == creature && GodotObject.IsInstanceValid(edge)) edge.Call("proc");
        }

        void OnCombatStateChanged(CombatState _)
        {
            if (GodotObject.IsInstanceValid(edge)) edge.Visible = creature.IsAlive;
        }

        EdgeProcced += OnProc;
        CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
        edge.TreeExiting += () =>
        {
            EdgeProcced -= OnProc;
            CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
        };
    }

    [HarmonyPatch(typeof(NMerchantRoom), "AfterRoomIsLoaded")]
    public static class MerchantPatch
    {
        private static readonly AccessTools.FieldRef<NMerchantRoom, List<Player>> Players =
            AccessTools.FieldRefAccess<NMerchantRoom, List<Player>>("_players");

        [HarmonyPostfix]
        public static void Postfix(NMerchantRoom __instance)
        {
            // AfterRoomIsLoaded creates one visual per player, in the same order
            var players = Players(__instance);
            var visuals = __instance.PlayerVisuals;
            for (var i = 0; i < Math.Min(players.Count, visuals.Count); i++)
            {
                var player = players[i];
                var character = visuals[i];

                if (Has<EdgeTheChao>(player))
                {
                    var edge = CreateEdge(EdgeMode.Merchant);
                    character.AddChild(edge);
                    edge.Position = EdgeMerchantPosition;
                }

                // Added last so she's drawn in front of Shion
                if (Has<AbbyTheAbsol>(player))
                {
                    var abby = CreateAbby("abby_merchant");
                    character.AddChild(abby);
                    abby.Position = AbbyMerchantPosition;
                }
            }
        }
    }

    [HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
    public static class RestSitePatch
    {
        [HarmonyPostfix]
        public static void Postfix(NRestSiteCharacter __instance)
        {
            // Under ControlRoot, so they get mirrored with the character (NRestSiteCharacter.FlipX)
            var root = __instance.GetNodeOrNull<Control>("ControlRoot");
            if (root == null || __instance.Player is not { } player) return;

            if (Has<EdgeTheChao>(player))
            {
                var edge = CreateEdge(EdgeMode.Rest);
                root.AddChild(edge);
                edge.Position = EdgeRestPosition;
            }

            if (Has<AbbyTheAbsol>(player))
            {
                var abby = CreateAbby("abby_rest_site");
                root.AddChild(abby);
                abby.Position = AbbyRestPosition;
            }
        }
    }
}
