using UnityEngine;

public class PlayerDashState:PlayerBaseState
{
    private float dashDuration = 0.22f;
    private float elapsedTime = 0f;
    private Vector3 dashDir = Vector3.zero;
    private float dashCooldown = 0.7f;
    private float lastDashTime = -Mathf.Infinity;

    public PlayerDashState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    private bool CanDash()
    {
        return Time.time >= lastDashTime + dashCooldown;
    }

    public override void StateEnter()
    {
        if(!CanDash())
        {
            stateMachine.ChangeState(stateMachine.MoveState);
            return;
        }

        if(!Player.Condition.UseStamina(15f))
        {
            stateMachine.ChangeState(stateMachine.MoveState);
            return;
        }

        StartAnimation(Player.AnimationData.DashParameterHash);
        
        base.StateEnter();

        lastDashTime = Time.time;
        SoundManager.Instance.PlaySFX(Player.transform.position, SoundAddressbleName.DashSound);

        elapsedTime = 0f;
        Player._Rigidbody.useGravity = false;
        Player._Rigidbody.velocity = Vector3.zero;

        dashDir = GetDashDirection();
        float dashPower = 15f;
        Player._Rigidbody.AddForce(dashDir * dashPower, ForceMode.VelocityChange);

        if(Player.DashVFXPrefab != null)
        {
            Vector3 spawnPosition = Player.transform.position + Vector3.up - dashDir * 0.8f;
            Quaternion rotation = Quaternion.LookRotation(dashDir);
            GameObject vfx = GameObject.Instantiate(Player.DashVFXPrefab, spawnPosition, rotation);
            GameObject.Destroy(vfx, 2f);
        }
    }

    private Vector3 GetDashDirection()
    {
        Vector2 input = Player.InputHandler.MoveInput;
        if(input.sqrMagnitude > 0.01f)
        {
            if(viewMode == ViewModeType.View2D)
                return new Vector3(input.x, 0, 0).normalized;
            else
            {
                var f = Camera.main.transform.forward; f.y = 0; f.Normalize();
                var r = Camera.main.transform.right; r.y = 0; r.Normalize();
                return (r * input.x + f * input.y).normalized;
            }
        }
        return Player.VisualTransform.forward;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= dashDuration)
        {
            if(Player.InputHandler.MoveInput != Vector2.zero)
                stateMachine.ChangeState(stateMachine.MoveState);
            else
                stateMachine.ChangeState(stateMachine.IdleState);
        }
    }

    public override void StateExit()
    {
        base.StateExit();
        Player._Rigidbody.velocity = Vector3.zero;
        StopAnimation(Player.AnimationData.MoveParameterHash);
        Player._Rigidbody.useGravity = true;
    }

    public override void StatePhysicsUpdate()
    {
        base.StatePhysicsUpdate();
    }

    protected override void PlayerLookAt()
    {
        Player.VisualTransform.forward = dashDir;
    }
}
