using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController:MonoBehaviour
{
    protected bool isInitialized = false; // ConditionData 초기화 여부 플래그

    [Header("ConditionData SO (엑셀 기반 SO 연결)")]
    [SerializeField] protected int ID;

    [Header("Visual Settings")]
    [SerializeField] protected Transform visualTransform;
    [SerializeField] protected float visualRotateSpeed = 10f;

    protected bool isPlaying = true;
    protected Vector3 velocityTmp;

    public Rigidbody _Rigidbody {  get; protected set; }
    public Animator _Animator { get; protected set; }
    public BoxCollider col {  get; protected set; }
    public BaseCondition Condition { get; protected set; } // 제네릭 타입으로 ConditionData 접근 
    public CombatController _CombatController { get; protected set; } // CombatController 접근
    public ViewChangeController _ViewChangeController { get; protected set; } // ViewChangeController 접근
    public bool IsInitialized => isInitialized; // ConditionData 초기화 여부 반환
    public Transform VisualTransform => visualTransform;
    public float VisualRotateSpeed => visualRotateSpeed;

    protected virtual void Awake()
    {
        StartCoroutine(WaitForDataLoad());
    }

    protected virtual void OnEnable()
    {
        GameManager.Instance.onGameStateChange += OnGameStateChange;
    }

    protected virtual void OnDisable()
    {
        if(GameManager.HasInstance)
            GameManager.Instance.onGameStateChange -= OnGameStateChange;
    }

    protected virtual void Start()
    {
        _CombatController = GetComponent<CombatController>();
        _ViewChangeController = GetComponent<ViewChangeController>();
        _Rigidbody = GetComponent<Rigidbody>();
        _Animator = GetComponentInChildren<Animator>();
        col = GetComponent<BoxCollider>();
    }


    protected virtual void Initialize()
    {
        Condition = new BaseCondition(InitConditionData());
    }

    /// <summary>
    /// ConditionData를 초기화합니다.
    /// BaseCondition을 상속받는 객체의 생성자에 전달
    /// </summary>
    /// <returns></returns>
    protected ConditionData InitConditionData()
    {
        ConditionData data = TableManager.Instance.GetTable<ConditionDataTable>().GetDataByID(ID);
        data.InitConditionDictionary();

        return data;
    }

    protected virtual IEnumerator WaitForDataLoad()
    {
        yield return new WaitUntil(() => GameManager.HasInstance && ViewManager.HasInstance && PoolManager.Instance.IsInitialized&&UIManager.Instance.IsUILoaded()&&SoundManager.Instance.IsOnLoad());
        Initialize();
    }

    public void init()
    {
        StartCoroutine(WaitForDataLoad());
    }
    /// <summary>
    /// 게임 상태가 변경될 때 호출되는 메서드입니다.
    /// </summary>
    protected void OnGameStateChange()
    {
        if(GameManager.Instance.curGameState == GameState.Play)
        {
            isPlaying = true;
        }
        else
        {
            isPlaying = false;
        }

        SetCharacterPauseMode(isPlaying);
    }

    /// <summary>
    /// 게임 상태에 따라 캐릭터의 일시정지 모드를 설정합니다.
    /// </summary>
    /// <param name="isPlaying"></param>
    protected virtual void SetCharacterPauseMode(bool isPlaying)
    {
        if(!isPlaying)
        {
            velocityTmp = _Rigidbody.velocity; // 현재 속도를 저장
            _Rigidbody.velocity = Vector3.zero; // 게임이 일시정지되면 Rigidbody 속도 초기화
            _Rigidbody.useGravity = false; // 중력 비활성화

            _Animator.speed = 0;
        }
        else
        {
            _Rigidbody.velocity = velocityTmp; // 게임이 일시정지되면 Rigidbody 속도 초기화
            velocityTmp = Vector3.zero; // 속도 초기화
            _Rigidbody.useGravity = true; // 중력 활성화

            _Animator.speed = 1;
        }
    }

    public abstract void Hit();

    public abstract void Die();

    protected virtual void OnDrawGizmosSelected()
    {
    }
}
