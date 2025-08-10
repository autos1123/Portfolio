public abstract class UnitStateMachine
{
    protected IUnitState currentState;

    public void ChangeState(IUnitState state)
    {
        currentState?.StateExit();
        currentState = state;
        currentState?.StateEnter();
    }
    public void Update()
    {
        currentState?.StateUpdate();
    }

    public void PhysicsUpdate()
    {
        currentState?.StatePhysicsUpdate();
    }
}
