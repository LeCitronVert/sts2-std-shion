using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using HarmonyLib;
using ShionTheBruiser.ShionTheBruiserCode.Enums;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Helper;
using ShionTheBruiser.ShionTheBruiserCode.Powers;

namespace ShionTheBruiser.ShionTheBruiserCode.Orbs;

/// <summary>
/// Channeled by Remote Body (Team-Up).
/// Gains 1 Charge whenever a Request is completed or another Orb is Evoked. Evokes itself at 5 Charges.
/// Evoke: at the start of your next turn, draw 5 cards, gain 5 energy and 5 Stars (<see cref="ShippingDelivery"/>).
/// </summary>
public class ShippingOrb : CustomOrbModel, IFryListener
{
    public const int MaxCharges = 5;
    public const int DeliveryAmount = 5;

    // The dark orb's rim is ~39px wide in its scene, so this fits the 256px big icon inside it.
    private const float IconScale = 0.13f;

    // Uses the Requested icon (not drawn yet: the orb shows without it until then).
    private static readonly string IconPath = "requested.png".BigPowerImagePath();

    public override string CustomIconPath => "requested.png".PowerImagePath();

    public override string CustomChannelSfx => "event:/sfx/characters/defect/defect_dark_channel";

    public override Color DarkenedColor => new("3a2a10");

    private int _charges;

    public override decimal PassiveVal => _charges;

    public override decimal EvokeVal => DeliveryAmount;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(FryCardKeywords.Request)];

    public override Node2D CreateCustomSprite()
    {
        var sprite = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath("orbs/orb_visuals/dark_orb")).Instantiate<Node2D>();
        if (sprite.GetNodeOrNull<CanvasItem>("SpineSkeleton") is { } skeleton)
        {
            skeleton.Modulate = Character.FryTheCommander.Color;
        }

        if (ResourceLoader.Exists(IconPath))
        {
            sprite.AddChild(new Sprite2D
            {
                Texture = ResourceLoader.Load<Texture2D>(IconPath),
                Scale = new Vector2(IconScale, IconScale),
            });
        }
        return sprite;
    }

    public Task AfterRequestCounterReachedZero(PlayerChoiceContext choiceContext, Requested request) => AddCharge(choiceContext);

    public override Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        return orb == this ? Task.CompletedTask : AddCharge(choiceContext);
    }

    private async Task AddCharge(PlayerChoiceContext choiceContext)
    {
        if (!IsInQueue() || _charges >= MaxCharges) return;

        _charges++;
        if (_charges < MaxCharges) return;

        await EvokeSelf(choiceContext);
    }

    private bool IsInQueue() => Owner.PlayerCombatState?.OrbQueue.Orbs.Contains(this) ?? false;

    private static readonly System.Reflection.MethodInfo EvokeMethod =
        AccessTools.Method(typeof(OrbCmd), "Evoke", [typeof(PlayerChoiceContext), typeof(Player), typeof(OrbModel), typeof(bool)]);

    /// <summary>Evokes this orb wherever it is in the queue (the game's OrbCmd.Evoke is private).</summary>
    private async Task EvokeSelf(PlayerChoiceContext choiceContext)
    {
        choiceContext.PushModel(this);
        await (Task)EvokeMethod.Invoke(null, [choiceContext, Owner, this, true])!;
        choiceContext.PopModel(this);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        PlayEvokeSfx();
        await PowerCmd.Apply<ShippingDelivery>(playerChoiceContext, Owner.Creature, 1, Owner.Creature, null);
        return [Owner.Creature];
    }
}
