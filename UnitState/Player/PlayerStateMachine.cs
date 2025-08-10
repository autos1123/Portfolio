public class PlayerStateMachine:UnitStateMachine
{
    public PlayerController Player { get; }

    public float MovementSpeed { get; private set; }

    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerAttack1State Attack1State { get; private set; }
    public PlayerAttack2State Attack2State { get; private set; }
    public PlayerAttack3State Attack3State { get; private set; }
    public PlayerSkillState SkillState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }
    public PlayerKnockBackState KnockbackState { get; private set; }



    public PlayerStateMachine(PlayerController player)
    {
        this.Player = player;
        MovementSpeed = Player.Condition.GetTotalCurrentValue(ConditionType.MoveSpeed);

        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        JumpState = new PlayerJumpState(this);
        DashState = new PlayerDashState(this);
        Attack1State = new PlayerAttack1State(this);
        Attack2State = new PlayerAttack2State(this);
        Attack3State = new PlayerAttack3State(this);
        SkillState = new PlayerSkillState(this);
        DeadState = new PlayerDeadState(this);
        KnockbackState = new PlayerKnockBackState(this);

        ChangeState(IdleState);
    }
}
