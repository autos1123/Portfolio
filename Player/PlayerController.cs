using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// 플레이어 이동, 공격, 점프, 데미지 처리 및 FSM 관리
/// </summary>
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class PlayerController:BaseController
{
    public PlayerInputHandler InputHandler { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerAnimationData AnimationData { get; private set; }
    public Animator Animator { get; private set; }
    public PlayerActiveItemController ActiveItemController { get; private set; }
    public Room CurrentRoom { get; private set; }

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float groundRayOffset = 0.1f;

    [Header("Dash VFX")]
    public GameObject DashVFXPrefab;

    public bool ComboBuffered { get; set; } = false;
    public Vector3 LastSafePosition { get; private set; }
    private Transform currentPlatform = null;
    private Vector3 platformLocalPosition;
    
    private float staminaDrainPerSecond = 5f;

    public Action OnSkillInput;

    private Vector3 lastSafePosition;


    public bool IsGrounded { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        InputHandler = GetComponent<PlayerInputHandler>();
        ActiveItemController = GetComponent<PlayerActiveItemController>();
        Animator = GetComponentInChildren<Animator>();
        _Rigidbody.freezeRotation = true;
    }

    private void Update()
    {
        if(!isInitialized || !isPlaying || Condition.IsDied)
            return;
        
        UpdateGrounded();
        UpdateSafePosition();
       

        StateMachine.Update();
        InputHandler.ResetOneTimeInputs();

        if(ViewManager.Instance.CurrentViewMode == ViewModeType.View3D)
        {
            if(Condition.CurrentConditions[ConditionType.Stamina] > 0f)
            {
                Condition.CurrentConditions[ConditionType.Stamina] -= staminaDrainPerSecond * Time.deltaTime;
                Condition.CurrentConditions[ConditionType.Stamina] = Mathf.Max(0f, Condition.CurrentConditions[ConditionType.Stamina]);
                Condition.statModifiers[ConditionType.Stamina]?.Invoke();
            }
            else
            {
                ViewManager.Instance.SwitchView(ViewModeType.View2D);
            }
        }
        else
        {
            // 2D일 때만 리젠!
            Condition.RegenerateStamina();
        }
    }

    private void FixedUpdate()
    {
        if(!isInitialized || !isPlaying)
            return;

        StateMachine.PhysicsUpdate();
    }

    /// <summary>
    /// 바닥 체크 (Raycast)
    /// </summary>
    private void UpdateGrounded()
    {
        Vector3 center = col.bounds.center;
        Vector3 extents = col.bounds.extents;

        // 아래 방향으로 약간 이동한 박스 중심 위치
        Vector3 boxCenter = center + Vector3.down * (extents.y + groundRayOffset * 0.5f);

        // 박스 크기 (살짝 얇게 Y축 조정)
        Vector3 boxHalfExtents = new Vector3(extents.x - 0.1f, groundRayOffset * 0.5f, extents.z - 0.1f);
    
        int combinedLayerMask = groundLayer | platformLayer;
        
        IsGrounded = Physics.OverlapBox(boxCenter, boxHalfExtents, Quaternion.identity, combinedLayerMask).Length > 0;

    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if(Application.isPlaying)
        {
            Vector3 center = col.bounds.center;
            Vector3 extents = col.bounds.extents;
            Vector3 boxCenter = center + Vector3.down * (extents.y + groundRayOffset * 0.5f);
            Vector3 boxHalfExtents = new Vector3(extents.x, groundRayOffset * 0.5f, extents.z);

            Gizmos.color = IsGrounded ? Color.blue : Color.black;
            Gizmos.DrawWireCube(boxCenter, boxHalfExtents * 2);
        }
    }

    /// <summary>
    /// 플레이어의 근접 공격 처리
    /// 주변 Enemy Layer 대상 충돌 검사 후 데미지 적용
    /// </summary>
    public void Attack()
    {
        Collider[] hitColliders = _CombatController.GetTargetColliders(LayerMask.GetMask("Enemy"));

        float attackPower = Condition.GetTotalCurrentValue(ConditionType.AttackPower);
        float criticalChance = Condition.GetTotalCurrentValue(ConditionType.CriticalChance);
        float criticalDamage = Condition.GetTotalCurrentValue(ConditionType.CriticalDamage);

        foreach(var hitCollider in hitColliders)
        {
            if(hitCollider.TryGetComponent(out IDamagable enemy))
            {
                // 크리티컬 판정
                bool isCritical = UnityEngine.Random.value < criticalChance;

                float finalDamage = attackPower;
                if(isCritical)
                {
                    finalDamage *= criticalDamage;
                }

                if(enemy.GetDamaged(finalDamage))
                {
                    DamageType damageType = isCritical ? DamageType.Critical : DamageType.Normal;
                    PoolingDamageUI damageUI = PoolManager.Instance.GetObject(PoolType.DamageUI).GetComponent<PoolingDamageUI>();
                    damageUI.InitDamageText(enemy.GetDamagedPos(), damageType, finalDamage);
                }
            }
        }
    }


    public override void Hit()
    {
        StateMachine.ChangeState(StateMachine.KnockbackState);
    }

    public override void Die()
    {
        StateMachine.ChangeState(StateMachine.DeadState);

        UIManager.Instance.ShowConfirmPopup(
            "사망했습니다! 로비로 돌아갑니다.",
            onConfirm: () => {
                Destroy(GameManager.Instance.Player);
                SceneManager.LoadScene("LobbyScene");
            },
            onCancel: null, 
            confirmText: "확인(Enter)" 
            );
    }

    /// <summary>
    /// FSM 및 PlayerCondition 초기화
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        AnimationData = new PlayerAnimationData();
        StateMachine = new PlayerStateMachine(this);

        UIManager.Instance.ShowUI<HUD>();



        // 인벤토리 초기화 
        Inventory inventory = GetComponent<Inventory>();
        if(inventory != null && !inventory.Initialized)
        {
            inventory.InitializeInventory(); // TableManager 준비될 때까지 대기 후 초기화
        }
        else
        {
            inventory.ApplyItemStat();
        }


            //temp
            EquipmentManager.Instance?.RefreshPlayer(); // 연동 강제 초기화

        isInitialized = true;
    }
    private void UpdateSafePosition()
    {
        Vector3 boxCenter = col.bounds.center + Vector3.down * (col.bounds.extents.y + 0.05f);
        Vector3 boxHalfExtents = new Vector3(col.bounds.extents.x * 0.9f, 0.05f, col.bounds.extents.z * 0.9f);
        int combinedLayer = groundLayer | platformLayer;

        Collider[] hits = Physics.OverlapBox(boxCenter, boxHalfExtents, Quaternion.identity, combinedLayer);

        if (hits.Length > 0)
        {
            LastSafePosition = transform.position;

            // 플랫폼일 경우 현재 플랫폼 정보 저장
            foreach (var hit in hits)
            {
                if (((1 << hit.gameObject.layer) & platformLayer) != 0)
                {
                    currentPlatform = hit.transform;
                    platformLocalPosition = currentPlatform.InverseTransformPoint(transform.position);
                    return;
                }
            }
            // 플랫폼이 아니라면
            currentPlatform = null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FallZone"))
        {
            ApplyFallDamage(20);
            if (!Condition.IsDied)
            {
                RespawnToSafePosition(); // 죽지 않았으면 리스폰
            }
        }
    }

    private void ApplyFallDamage(float damageAmount)
    {
        if (Condition.IsDied) return; // 이미 죽었으면 낙사 데미지 무시

        bool isDead = Condition.GetDamaged(damageAmount);
        if (isDead)
        {
            Die(); // PlayerController에 있는 Die() 함수 호출
        }
    }
    
    public void RespawnToSafePosition()
    {
        if (Condition.CurrentConditions[ConditionType.HP] <= 0)
        {
            Debug.Log("사망 상태 -> 리스폰 불가");
            return;
        }
        Vector3 respawnPosition = (currentPlatform != null)
            ? currentPlatform.TransformPoint(platformLocalPosition)
            : LastSafePosition;

        _Rigidbody.velocity = Vector3.zero;
        transform.position = respawnPosition + Vector3.up * 1f; // 살짝 띄워서 리스폰
    }
}
