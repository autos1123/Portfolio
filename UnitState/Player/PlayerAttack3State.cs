using System.Linq;
using UnityEngine;

public class PlayerAttack3State:PlayerBaseState
{
    private float timer = 0f;
    private float actualClipLength = 0f;

    public PlayerAttack3State(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void StateEnter()
    {
        base.StateEnter();

        float attackSpeed = Player.Condition.GetTotalCurrentValue(ConditionType.AttackSpeed);
        Player.Animator.SetFloat("AttackSpeed", attackSpeed);

        float baseClipLength = Player.Animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(x => x.name == "Clap Attack").length;
        actualClipLength = baseClipLength / attackSpeed;

        timer = 0f;

        StartAnimation(Player.AnimationData.Attack3ParameterHash);
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        timer += Time.deltaTime;
        var input = Player.InputHandler;

        if(input.DashPressed)
        {
            stateMachine.ChangeState(stateMachine.DashState);
            return;
        }
        if(input.JumpPressed && Player.IsGrounded)
        {
            stateMachine.ChangeState(stateMachine.JumpState);
            return;
        }
        if(input.SkillQPressed)
        {
            stateMachine.SkillState.SetSkill(Skillinput.X);
            stateMachine.ChangeState(stateMachine.SkillState);
            return;
        }
        if(input.SkillEPressed)
        {
            stateMachine.SkillState.SetSkill(Skillinput.C);
            stateMachine.ChangeState(stateMachine.SkillState);
            return;
        }

        if(timer > actualClipLength)
            stateMachine.ChangeState(stateMachine.IdleState);
    }

    public override void StateExit() { base.StateExit(); }
    public override void StatePhysicsUpdate()
    {
        base.StatePhysicsUpdate();
        Move(Player.InputHandler.MoveInput);
        
    }
}
