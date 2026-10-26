using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using ShionTheBruiser.ShionTheBruiserCode.Character;

namespace ShionTheBruiser.ShionTheBruiserCode.Singletons;

public class SlalApparenceSingleton(): CustomSingletonModel(HookType.Combat)
{
    private const float MinScale = 0.4f;
    private const float MaxScale = 1f;
    
    public override async Task AfterCurrentHpChanged(Creature creature, Decimal delta)
    {
        await base.AfterCurrentHpChanged(creature, delta);
        if (creature.Player?.Character is not SlalTheEndless) return;
        UpdateScale(creature);
    }

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();
        // DebugOnlyGetState just returns the live combat state; BeforeCombatStart has no other way to reach it.
        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null) return;
        foreach (var creature in combatState.Allies)
        {
            if (creature.Player?.Character is SlalTheEndless) UpdateScale(creature);
        }
    }

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        await base.AfterCreatureAddedToCombat(creature);
        if (creature.Player?.Character is SlalTheEndless) UpdateScale(creature);
    }

    private void UpdateScale(Creature creature)
    {
        float ratio = creature.MaxHp > 0
            ? Mathf.Clamp((float)creature.CurrentHp / creature.MaxHp, 0f, 1f)
            : 0f;
        float scale = Mathf.Lerp(MinScale, MaxScale, ratio);
        NCombatRoom.Instance?.GetCreatureNode(creature)?.ScaleTo(scale, 0.4);
    }
}