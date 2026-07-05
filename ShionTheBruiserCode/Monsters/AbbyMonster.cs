using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ShionTheBruiser.ShionTheBruiserCode.Monsters;

public class AbbyMonster: CustomMonsterModel
{
    public override int MinInitialHp => 9999;

    public override int MaxInitialHp => 9999;

    public override bool IsHealthBarVisible => false;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState initialState = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        initialState.FollowUpState = initialState;

        return new MonsterMoveStateMachine(
            [initialState],
            initialState
        );
    }
    
    public override NCreatureVisuals? CreateCustomVisuals()
    {
        return NodeFactory<NCreatureVisuals>.CreateFromResource(Path.Join(MainFile.ResPath, "images", "monsters", "abby_sprite.png"));
    }

}