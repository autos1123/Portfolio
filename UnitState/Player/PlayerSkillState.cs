using UnityEngine;

public class PlayerSkillState:PlayerBaseState
{
    private Skillinput usingSkill = Skillinput.None;

    public PlayerSkillState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public void SetSkill(Skillinput skill)
    {
        usingSkill = skill;
    }

    public void UseSkill()
    {
        Player.ActiveItemController.UseItem(usingSkill);
    }

    public override void StateEnter()
    {
        base.StateEnter();
        Vector2 move = Player.InputHandler.MoveInput;
        if(!Player.ActiveItemController.CanUseSkill(usingSkill))
        {
            if(move != Vector2.zero)
                stateMachine.ChangeState(stateMachine.MoveState);
            else
                stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }
        Player.OnSkillInput += UseSkill;
        StartAnimation(Player.AnimationData.SkillParameterHash);


    }


    public override void StateUpdate()
    {
        base.StateUpdate();
        if(Player._Animator.GetCurrentAnimatorStateInfo(0).IsName("Cast") &&
            Player._Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
    }

    public override void StateExit()
    {
        base.StateExit();
        Player.OnSkillInput -= UseSkill;
        StopAnimation(Player.AnimationData.SkillParameterHash);
    }

    public override void StatePhysicsUpdate()
    {
        base.StatePhysicsUpdate();
        if(Player.IsGrounded)
        {
            Player._Rigidbody.velocity = Vector3.zero; // 지상에서만 고정
        }
    }
}
