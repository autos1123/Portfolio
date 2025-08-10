using UnityEngine;

public class PlayerBaseState:IUnitState
{
    protected PlayerStateMachine stateMachine;
    protected ConditionData data;
    protected ViewModeType viewMode;

    protected PlayerController Player => stateMachine.Player;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        data = stateMachine.Player.Condition.Data;
    }

    public virtual void StateEnter()
    {
        if(ViewManager.HasInstance)
        {
            viewMode = ViewManager.Instance.CurrentViewMode;
            ViewManager.Instance.OnViewChanged += SwitchView;
        }
        else
        {
            viewMode = ViewModeType.View2D;
        }
    }

    public virtual void StateExit()
    {
        if(ViewManager.HasInstance)
        {
            ViewManager.Instance.OnViewChanged -= SwitchView;
        }
    }

    public virtual void StatePhysicsUpdate()
    {
        // 자식 클래스에서 오버라이드
    }

    public virtual void StateUpdate()
    {
        PlayerLookAt();
    }

    protected void StartAnimation(int animationHash)
    {
        Player._Animator.SetBool(animationHash, true);
    }

    protected void StopAnimation(int animationHash)
    {
        Player._Animator.SetBool(animationHash, false);
    }

    public void SwitchView(ViewModeType mode)
    {
        if(viewMode == mode) return;
        viewMode = mode;
    }

    /// <summary>
    /// 플레이어 이동 처리 (상태에서 호출)
    /// </summary>
    protected Vector3 Move(Vector2 input)
    {
        float speed = Player.Condition.GetTotalCurrentValue(ConditionType.MoveSpeed);

        Vector3 dir;
        if(viewMode == ViewModeType.View2D)
        {
            var r = Camera.main.transform.right; r.y = 0; r.Normalize();
            dir = r * input.x;
        }
        else
        {
            var f = Camera.main.transform.forward; f.y = 0; f.Normalize();
            var r = Camera.main.transform.right; r.y = 0; r.Normalize();
            dir = r * input.x + f * input.y;
        }

        Vector3 delta = dir * speed;
        delta.y = Player._Rigidbody.velocity.y;
        Player._Rigidbody.velocity = delta;
        return dir;
    }

    protected virtual void PlayerLookAt()
    {
        // 3D 시점 회전
        if(viewMode == ViewModeType.View3D)
        {
            Plane plane = new Plane(Vector3.up, Player.transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float enter;
            if(plane.Raycast(ray, out enter))
            {
                Vector3 mouseWorld = ray.GetPoint(enter);
                Vector3 dir = (mouseWorld - Player.transform.position);
                dir.y = 0;
                if(dir.sqrMagnitude > 0.01f)
                    Player.VisualTransform.forward = dir.normalized;
            }
        }
        // 2D 시점 회전
        else if(viewMode == ViewModeType.View2D)
        {
            Vector3 mouseScreen = Input.mousePosition;
            mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z - Player.transform.position.z);
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            Vector3 dir = (mouseWorld - Player.transform.position);
            dir.y = 0;
            if(Mathf.Abs(dir.x) > 0.01f)
                Player.VisualTransform.forward = new Vector3(Mathf.Sign(dir.x), 0, 0);
        }
    }
}