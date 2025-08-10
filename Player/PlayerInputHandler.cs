using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 입력을 처리하고, ViewMode(2D/3D)에 따라 분기하지 않고 원본 입력을 유지.
/// 이동 방향 분기는 실제 이동 처리에서 적용.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler:MonoBehaviour
{
    private PlayerInputActions inputActions;

    /// <summary>
    /// 이동 방향 입력 (Vector2)
    /// 2D: (좌우), 3D: (좌우 + 상하)
    /// </summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>
    /// Jump 버튼이 눌렸는지 여부 (한 프레임만 true)
    /// </summary>
    public bool JumpPressed { get; private set; }

    /// <summary>
    /// Attack 버튼이 눌렸는지 여부 (한 프레임만 true)
    /// </summary>
    public bool AttackPressed { get; private set; }

    ///<summary>
    /// Dash 버튼이 눌렸는지 여부 (한 프레임만 true)
    /// </summary>
    public bool DashPressed { get; private set; }

    /// <summary>
    /// 스킬 X 버튼이 눌렸는지 여부 (한 프레임만 true)
    /// </summary>
    public bool SkillQPressed { get; private set; }
    /// <summary>
    /// 스킬 C 버튼이 눌렸는지 여부 (한 프레임만 true)
    /// </summary>
    public bool SkillEPressed { get; private set; }

    private float dashCooldown = 0.7f;
    private float lastDashTime = -Mathf.Infinity;

    /// <summary>
    /// 상호작용 키 입력 시 호출되는 이벤트
    /// </summary>
    public Action<InputAction.CallbackContext> OnInteraction;


    private void Awake()
    {
        inputActions = new PlayerInputActions();

        // 방향키, WASD 등의 방향 입력 등록
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        // 점프, 공격, 대쉬: 한 프레임만 true로 처리
        inputActions.Player.Jump.performed += ctx => JumpPressed = true;
        inputActions.Player.Attack.performed += ctx =>
        {
            if(UIManager.Instance.uiStack.Count == 0) AttackPressed = true;
        };
        inputActions.Player.Dash.performed += ctx => {
            if(Time.time >= lastDashTime + dashCooldown)
            {
                DashPressed = true;
                lastDashTime = Time.time; // 대쉬 쿨타임 갱신
            }
        };
        inputActions.Player.UseQItem.performed += ctx => SkillQPressed = true;
        inputActions.Player.UseEitem.performed += ctx => SkillEPressed = true;

        inputActions.Player.ChangeView.performed += ctx =>
        {   //v키 입력이 허용되지않았다면
            if (TutorialManager.HasInstance && !TutorialManager.Instance.IsViewChangeInputAllowed())
            {
                Debug.Log("V키 입력 막혀있음 (튜토리얼)");
                return;
            }
            ViewManager.Instance.ToggleView();
        };
        inputActions.Player.OpenOptions.performed += OnOpenOption;
        inputActions.Player.OpenInventory.performed += ctx => UIManager.Instance.ToggleUI<InventoryUI>();
        inputActions.Player.OpenStatus.performed += ctx => UIManager.Instance.ToggleUI<StatusBoard>();
        inputActions.Player.OpenMinmap.performed += ctx => UIManager.Instance.ToggleUI<MinimapUI>();
        
        // 상호작용 입력 (F키)
        inputActions.Player.Interaction.performed += OnInteraction;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    /// <summary>
    /// 방향 입력 처리 (입력 중 또는 입력 취소 시 둘 다 호출됨)
    /// </summary>
    private void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        if(ViewManager.Instance.CurrentViewMode == ViewModeType.View2D)
        {
            // 2D 모드에서는 y축 입력을 무시
            MoveInput = new Vector2(MoveInput.x, 0);
        }
        // 입력이 취소되면 (0, 0)이 자동으로 들어옴
    }

    /// <summary>
    /// Jump, Attack 같은 일회성 입력값을 초기화
    /// 매 프레임 후 호출 필요
    /// </summary>
    public void ResetOneTimeInputs()
    {
        JumpPressed = false;
        AttackPressed = false;
        DashPressed = false;
        SkillQPressed = false;
        SkillEPressed = false;
    }

    public bool IsPressingDown()
    {
        return MoveInput.y < -0.5f;
    }

    public bool TestDamageKeyPressed()
    {
        return Keyboard.current.hKey.wasPressedThisFrame;
    }

    private void OnOpenOption(InputAction.CallbackContext context)
    {
        
        UIManager.Instance.CloseOption();
        
        if(UIManager.Instance.GetUI<OptionBoard>().gameObject.activeSelf == true)
        {
            GameManager.Instance.setState(GameState.Stop);            
        }
        else
        {
            GameManager.Instance.setState(GameState.Play);
            
        }
    }
    
    public void SetInputEnabled(bool isEnabled)
    {
        if (isEnabled)
            inputActions.Player.Enable();
        else
            inputActions.Player.Disable();
    }
}
