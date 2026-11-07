using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Helper;

namespace ShionTheBruiser.ShionTheBruiserCode.Visuals;

/// <summary>
/// Shows a small golden butterfly (scenes/fx/novel_butterfly.tscn) next to the energy cost of Novel / Always Novel cards.
/// Purely visual: it follows the card model's keywords, so it disappears as soon as Novel is consumed.
/// </summary>
[HarmonyPatch(typeof(NCard))]
public static class NovelButterflyPatch
{
    private static readonly string ScenePath = Path.Join(MainFile.ResPath, "scenes", "fx", "novel_butterfly.tscn");
    private const string NodeName = "NovelButterfly";

    private static readonly AccessTools.FieldRef<NCard, TextureRect> EnergyIconRef =
        AccessTools.FieldRefAccess<NCard, TextureRect>("_energyIcon");

    private sealed class Binding
    {
        public CardModel? Model;
        public Action? Handler;
    }

    private static readonly ConditionalWeakTable<NCard, Binding> Bindings = new();

    [HarmonyPostfix]
    [HarmonyPatch("Reload")]
    public static void ReloadPostfix(NCard __instance)
    {
        Bind(__instance);
        Refresh(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NCard.UpdateVisuals))]
    public static void UpdateVisualsPostfix(NCard __instance) => Refresh(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NCard._EnterTree))]
    public static void EnterTreePostfix(NCard __instance)
    {
        Bind(__instance);
        Refresh(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(NCard._ExitTree))]
    public static void ExitTreePostfix(NCard __instance) => Unbind(__instance);

    /// <summary>Listens to the model's keyword changes so the butterfly reacts when Novel is added or consumed.</summary>
    private static void Bind(NCard card)
    {
        var binding = Bindings.GetOrCreateValue(card);
        var model = card.Model;
        if (binding.Model == model && binding.Handler != null) return;

        Unbind(card);
        if (model == null || !card.IsInsideTree()) return;

        binding.Model = model;
        binding.Handler = () =>
        {
            if (GodotObject.IsInstanceValid(card)) Refresh(card);
        };
        model.KeywordsChanged += binding.Handler;
    }

    private static void Unbind(NCard card)
    {
        if (!Bindings.TryGetValue(card, out var binding)) return;
        if (binding.Model != null && binding.Handler != null)
        {
            binding.Model.KeywordsChanged -= binding.Handler;
        }
        binding.Model = null;
        binding.Handler = null;
    }

    private static void Refresh(NCard card)
    {
        var icon = EnergyIconRef(card);
        if (icon == null || !GodotObject.IsInstanceValid(icon)) return; // Not ready yet
        var parent = icon.GetParent();
        if (parent == null) return;

        var model = card.Model;
        var show = model != null
                   && card.Visibility == ModelVisibility.Visible
                   && SlalMechanicsHelper.IsNovel(model);

        var butterfly = parent.GetNodeOrNull<Node2D>(NodeName);
        if (!show)
        {
            if (butterfly != null) butterfly.Visible = false;
            return;
        }

        if (butterfly == null)
        {
            butterfly = GD.Load<PackedScene>(ScenePath).Instantiate<Node2D>();
            butterfly.Name = NodeName;
            parent.AddChild(butterfly);
        }

        // Perched on the upper-right edge of the energy orb
        butterfly.Position = icon.Position + new Vector2(icon.Size.X * 0.98f, icon.Size.Y * 0.22f);
        butterfly.Visible = true;
    }
}
