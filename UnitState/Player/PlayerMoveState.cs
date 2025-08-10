using UnityEngine;

/// <summary>
/// 플레이어 이동 상태
/// </summary>
public class PlayerMoveState:PlayerBaseState
{
    private float inputDelay = 0f; // 입력 지연 시간
    private float inputDelayTime = 0.1f; // 입력 지연 시간 설정
    public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    
    public override void StateEnter()
    {
        base.StateEnter();
        StartAnimation(Player.AnimationData.MoveParameterHash);
    }

    public override void StateExit()
    {
        base.StateExit();
        StopAnimation(Player.AnimationData.MoveParameterHash);
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        Vector2 move = Player.InputHandler.MoveInput;

        if(Player.InputHandler.JumpPressed && Player.IsGrounded)
        {
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }

        if(Player.InputHandler.DashPressed)
        {
            stateMachine.ChangeState(stateMachine.DashState);
        }
        if(Player.InputHandler.AttackPressed)
        {
            stateMachine.ChangeState(stateMachine.Attack1State);
        }
        if(Player.InputHandler.SkillQPressed)
        {
            stateMachine.SkillState.SetSkill(Skillinput.X);
            stateMachine.ChangeState(stateMachine.SkillState);
            
        }
        if(Player.InputHandler.SkillEPressed)
        {
            stateMachine.SkillState.SetSkill(Skillinput.C);
            stateMachine.ChangeState(stateMachine.SkillState);
            
        }

        if(move != Vector2.zero)
        {
            inputDelay = 0f;
            PlayerLookAt();
        }
        else
        {
            inputDelay += Time.deltaTime;
            if(inputDelay >= inputDelayTime)
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
        }
    }

    public override void StatePhysicsUpdate()
    {
        base.StatePhysicsUpdate();
        Move(Player.InputHandler.MoveInput);
    }
}
