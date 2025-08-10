using System.Linq;
using UnityEngine;

public class PlayerAttack1State:PlayerBaseState
{
    private float comboTimer = 0f;
    private float comboWindowStart = 0f;
    private float comboWindowEnd = 0f;
    private float actualClipLength = 0f;

    private const float MinComboWindow = 0.18f; // 할로우나이트 느낌에 가깝게

    public PlayerAttack1State(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void StateEnter()
    {
        base.StateEnter();

        // 콤보 버퍼, 캔슬 모두 초기화
        Player.ComboBuffered = false;

        float attackSpeed = Player.Condition.GetTotalCurrentValue(ConditionType.AttackSpeed);
        Player.Animator.SetFloat("AttackSpeed", attackSpeed);

        float baseClipLength = Player.Animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(x => x.name == "Slap Attack").length;
        actualClipLength = baseClipLength / attackSpeed;

        // 콤보 가능 구간, 캔슬 구간
        comboWindowStart = actualClipLength * 0.20f;
        comboWindowEnd = Mathf.Max(comboWindowStart + MinComboWindow, actualClipLength * 0.7f);
        comboWindowEnd = Mathf.Min(comboWindowEnd, actualClipLength);

        comboTimer = 0f;

        StartAnimation(Player.AnimationData.Attack1ParameterHash);
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        comboTimer += Time.deltaTime;

        var input = Player.InputHandler;
        // 콤보 윈도우 진입
        if(comboTimer >= comboWindowStart && comboTimer <= comboWindowEnd)
        {

            // Attack 입력시 콤보 버퍼 처리 (프레임 단위 입력)
            if(input.AttackPressed)
                Player.ComboBuffered = true;

            // 콤보 버퍼가 들어오면 바로 Attack2State로!
            if(Player.ComboBuffered)
            {
                Player.ComboBuffered = false;
                stateMachine.ChangeState(stateMachine.Attack2State);
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

    public override void StateExit()
    {
        base.StateExit();
    }

    public override void StatePhysicsUpdate()
    {
        base.StatePhysicsUpdate();
            Move(Player.InputHandler.MoveInput);
    }
}
