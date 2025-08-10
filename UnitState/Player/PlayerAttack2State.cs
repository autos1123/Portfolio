using System.Linq;
using UnityEngine;

public class PlayerAttack2State:PlayerBaseState
{
    private float comboTimer = 0f;
    private float comboWindowStart = 0f;
    private float comboWindowEnd = 0f;
    private float actualClipLength = 0f;
    private const float MinComboWindow = 0.18f;

    public PlayerAttack2State(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void StateEnter()
    {
        base.StateEnter();

        Player.ComboBuffered = false;
        float attackSpeed = Player.Condition.GetTotalCurrentValue(ConditionType.AttackSpeed);
        Player.Animator.SetFloat("AttackSpeed", attackSpeed);

        float baseClipLength = Player.Animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(x => x.name == "Slash Attack").length;
        actualClipLength = baseClipLength / attackSpeed;

        comboWindowStart = actualClipLength * 0.18f;
        comboWindowEnd = Mathf.Max(comboWindowStart + MinComboWindow, actualClipLength * 0.75f);
        comboWindowEnd = Mathf.Min(comboWindowEnd, actualClipLength);

        comboTimer = 0f;

        StartAnimation(Player.AnimationData.Attack2ParameterHash);
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        comboTimer += Time.deltaTime;

        var input = Player.InputHandler;

        if(comboTimer >= comboWindowStart && comboTimer <= comboWindowEnd)
        {
            if(input.AttackPressed)
                Player.ComboBuffered = true;
            if(Player.ComboBuffered)
            {
                Player.ComboBuffered = false;
                stateMachine.ChangeState(stateMachine.Attack3State);
                return;
            }
        }
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

        if(comboTimer > actualClipLength)
            stateMachine.ChangeState(stateMachine.IdleState);
    }

    public override void StateExit() { base.StateExit(); }
    public override void StatePhysicsUpdate()
    {
        base.StatePhysicsUpdate();
            Move(Player.InputHandler.MoveInput);
        
    }
}
