using UnityEngine;

public class PlayerJumpState:PlayerBaseState
{
    private float jumpStartTime;
    private float jumpGroundGrace = 0.05f; // 점프 후 ground check 무시 시간
    public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void StateEnter()
    {
        base.StateEnter();

       StartAnimation(Player.AnimationData.JumpParameterHash);

        float force = Player.Condition.GetTotalCurrentValue(ConditionType.JumpPower);

        Vector3 v = Player._Rigidbody.velocity;

        v.y = 0;
        Player._Rigidbody.velocity = v;

        Player._Rigidbody.AddForce(Vector3.up * force, ForceMode.Impulse);

        jumpStartTime = Time.time;
    }


    public override void StateExit()
    {
        base.StateExit();
        StopAnimation(Player.AnimationData.JumpParameterHash);
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        Vector2 move = Player.InputHandler.MoveInput;
        if(move.magnitude > 0.1f)
        {
            PlayerLookAt();
        }

        // jumpGroundGrace 동안 낙하/착지 체크 안함
        if(Time.time - jumpStartTime < jumpGroundGrace)
            return;

        if(Player.IsGrounded)
        {
            if(move != Vector2.zero)
                stateMachine.ChangeState(stateMachine.MoveState);
            else
                stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }
        if(Player.InputHandler.AttackPressed)
        {
            stateMachine.ChangeState(stateMachine.Attack1State);
            return;
        }
        if(Player.InputHandler.DashPressed)
        {
            stateMachine.ChangeState(stateMachine.DashState);
            return;
        }
        if(Player.InputHandler.SkillQPressed)
        {
            stateMachine.SkillState.SetSkill(Skillinput.X);
            stateMachine.ChangeState(stateMachine.SkillState);
            return;
        }
        if(Player.InputHandler.SkillEPressed)
        {
            stateMachine.SkillState.SetSkill(Skillinput.C);
            stateMachine.ChangeState(stateMachine.SkillState);
            return;
        }
    }


    public override void StatePhysicsUpdate()
    {
        base.StatePhysicsUpdate();
        Move(Player.InputHandler.MoveInput);
    }
}
