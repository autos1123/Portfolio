using UnityEngine;

/// <summary>
/// 플레이어 사망 상태
/// </summary>
public class PlayerDeadState:PlayerBaseState
{

    public PlayerDeadState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void StateEnter()
    {
        base.StateEnter();
        if(Player.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Player.enabled = false;

        StartAnimation(Player.AnimationData.DeadParameterHash);
    }

    public override void StateExit()
    {
        base.StateExit();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
    }

    public override void StatePhysicsUpdate()
    {
       base.StatePhysicsUpdate();
    }
}
