using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using ShionTheBruiser.ShionTheBruiserCode.Cards;
using ShionTheBruiser.ShionTheBruiserCode.Extensions;
using ShionTheBruiser.ShionTheBruiserCode.Powers;
using IntentCategory = ShionTheBruiser.ShionTheBruiserCode.Cards.ShionInstinctualPremonitions.IntentCategory;

namespace ShionTheBruiser.ShionTheBruiserCode.Orbs;

/// <summary>
/// Channeled by Watch and Learn (Team-Up).
/// Passive (start of turn): Instinctual Premonitions' effects, for each enemy intent.
/// Evoke: ALL of Instinctual Premonitions' effects once.
/// Focus affects the Block.
/// </summary>
public class MonsterOrb : CustomOrbModel
{
    private const float IconScale = 0.3f;

    public override string CustomIconPath => "monstermancy.png".PowerImagePath();

    public override string CustomChannelSfx => "event:/sfx/characters/defect/defect_dark_channel";

    public override Color DarkenedColor => new("4a1a75");

    public override decimal PassiveVal => ModifyOrbValue(ShionInstinctualPremonitions.BlockBonusAgainstAttacks);

    public override decimal EvokeVal => ModifyOrbValue(ShionInstinctualPremonitions.BlockBonusAgainstAttacks);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<CounterStrategy>(),
    ];

    // A purple Dark Orb with Monstermancy's icon on top.
    public override Node2D CreateCustomSprite()
    {
        var sprite = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath("orbs/orb_visuals/dark_orb")).Instantiate<Node2D>();
        if (sprite.GetNodeOrNull<CanvasItem>("SpineSkeleton") is { } skeleton)
        {
            skeleton.Modulate = Character.ShionTheBruiser.Color;
        }

        var icon = new Sprite2D
        {
            Texture = PreloadManager.Cache.GetTexture2D("monstermancy.png".BigPowerImagePath()),
            Scale = new Vector2(IconScale, IconScale),
        };
        sprite.AddChild(icon);
        return sprite;
    }

    public override async Task AfterTurnStartOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target != null)
        {
            throw new InvalidOperationException("Monster orbs cannot target creatures.");
        }
        Trigger();
        PlayPassiveSfx();

        foreach (var intent in ShionInstinctualPremonitions.GetEnemyIntents(Owner.Creature.CombatState))
        {
            await GainEffect(choiceContext, ShionInstinctualPremonitions.Categorize(intent), PassiveVal);
        }
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
    {
        PlayEvokeSfx();
        foreach (var category in Enum.GetValues<IntentCategory>())
        {
            await GainEffect(playerChoiceContext, category, EvokeVal);
        }
        return [Owner.Creature];
    }

    private async Task GainEffect(PlayerChoiceContext choiceContext, IntentCategory category, decimal block)
    {
        var owner = Owner.Creature;
        switch (category)
        {
            case IntentCategory.Attack:
                await CreatureCmd.GainBlock(owner, block, ValueProp.Unpowered, null);
                break;
            case IntentCategory.Debuff:
                await PowerCmd.Apply<CounterStrategy>(choiceContext, owner, ShionInstinctualPremonitions.CounterStrategyAgainstDebuffs, owner, null);
                break;
            default:
                await PlayerCmd.GainEnergy(ShionInstinctualPremonitions.EnergyBonusAgainstOthers, Owner);
                break;
        }
    }
}
