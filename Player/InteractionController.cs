using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionController:MonoBehaviour
{
    private PlayerController player;
    private PlayerInputHandler inputHandler;
    private Collider col;

    [Header("Interaction")]
    private IInteractable interactableObj;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactableRange = 2.0f;
    [SerializeField] private Transform interactTextTr;
    private TextMeshProUGUI interactText;
    private GameObject lastInteractedNPC;
    
    private Vector3 size_2D;

    //interactTextTr에 외부에서 접근할 수 있도록
    public Transform GetInteractTextTransform()
    {
        return interactTextTr;
    }
    private void Awake()
    {
        if(interactTextTr != null) // null 체크 추가
        {
            interactText = interactTextTr.GetComponentInChildren<TextMeshProUGUI>();
        }
        inputHandler = GetComponent<PlayerInputHandler>();
        col = GetComponent<Collider>(); // Collider 컴포넌트 가져오기
        player = GetComponent<PlayerController>(); // 플레이어 메쉬 트랜스폼 가져오기
    }
    private void OnEnable()
    {
        if(inputHandler != null)
        {
            inputHandler.OnInteraction += OnInteractableAction;
        }

        InvokeRepeating(nameof(InteractableCheck), 0f, 0.2f);
    }

    private void OnDisable()
    {
        if(inputHandler != null)
        {
            inputHandler.OnInteraction -= OnInteractableAction;
        }

        CancelInvoke(nameof(InteractableCheck));
    }

    /// <summary>
    /// 외부에서 interactTextTr의 부모를 플레이어로 되돌리는 메서드.
    /// </summary>
    public void SetInteractTextParentToPlayer()
    {
        if (interactTextTr != null && transform != null)
        {
            interactTextTr.SetParent(transform, false);
            interactTextTr.gameObject.SetActive(false); 
        }
    }
    
    /// <summary>
    /// 상호작용 키 입력 시 호출되는 메서드
    /// 플레이어 중심으로 상호작용 오브젝트 탐색 후 상호작용 메서드 호출
    /// </summary>
    /// <param name="context"></param>
    public void OnInteractableAction(InputAction.CallbackContext context)
    {
        // 상호작용 오브젝트가 없거나, 비활성화된 경우 무시
        if(interactableObj == null || !IsValidInteractable(interactableObj))
            return;

        if(interactableObj.CanInteract(gameObject))
        {
            interactableObj.Interact(gameObject);
            
            // 마지막으로 상호작용한 NPC 저장
            var mono = interactableObj as MonoBehaviour;
            if (mono != null)
            {
                lastInteractedNPC = mono.gameObject;
            }
            
            interactableObj = null;
            
            InteractableCheck(); 
        }
    }

    private void InteractableCheck()
    {
        if (interactableObj != null && !IsValidInteractable(interactableObj))
        {
            interactableObj = null;
            if (interactTextTr != null && interactTextTr.gameObject.activeSelf)
            {
                SetInteractTextParentToPlayer();
            }
        }
        if(interactTextTr == null) return; // 텍스트 Transform이 없으면 더 이상 진행하지 않음
        // 상호작용 오브젝트 탐색
        Collider[] hitColliders;

        if(ViewManager.Instance.CurrentViewMode == ViewModeType.View3D)
        {
            hitColliders = Physics.OverlapSphere(
                transform.position,
                interactableRange,
                interactableLayer
            );
        }
        else
        {
            size_2D = new Vector3(interactableRange, col.bounds.extents.y, col.bounds.extents.z);
            hitColliders = Physics.OverlapBox(col.bounds.center, size_2D, Quaternion.identity, interactableLayer);
        }

        IInteractable currentBestInteractable = null;
        float minDistanceSq = float.MaxValue;
        for(int i = 0; i < hitColliders.Length; i++)
        {
            if(hitColliders[i].TryGetComponent(out IInteractable interactable) && IsValidInteractable(interactable))
            {
                if(IsValidInteractable(interactable))
                {
                    float distSq = (ViewManager.Instance.CurrentViewMode == ViewModeType.View3D) ? (hitColliders[i].transform.position - transform.position).sqrMagnitude : Mathf.Abs(hitColliders[i].transform.position.x - transform.position.x);

                    if(distSq < minDistanceSq)
                    {
                        minDistanceSq = distSq;
                        currentBestInteractable = interactable;
                    }
                }
            }
        }
        
        // 대상 교체가 일어났다면
        if(interactableObj != currentBestInteractable)
        {
            interactableObj = currentBestInteractable;
        }

        // 대상이 없으면 UI 초기화 후 종료
        if(interactableObj == null) //비어 있으면 초기화
        {
            if(interactTextTr.gameObject.activeSelf) // 이미 비활성화되어 있지 않은 경우에만
            {
                SetInteractTextParentToPlayer();
            }
            CloseNPCRelatedUIs();
            lastInteractedNPC = null;
            return;
        }
        
        //대상이 NPC가 아니고, 이전 NPC와 달라졌다면
        if (interactableObj != null)
        {
            var newTarget = interactableObj as MonoBehaviour;
            if (newTarget != null && lastInteractedNPC != null && newTarget.gameObject != lastInteractedNPC)
            {
                CloseNPCRelatedUIs();
                lastInteractedNPC = null;
            }
        }

        // 대상이 상호작용 가능하면 UI 표시
        if(interactableObj.CanInteract(gameObject)) // 옮겨 주기
        {
            SetInteractTextTransform(interactableObj.PromptPivot, interactableObj.InteractionPrompt);
            interactTextTr.gameObject.SetActive(true); // 활성화
        }
        else
        {
            // 상호작용은 가능하지만 지금은 할 수 없는 상태일 경우 UI를 숨깁
            if (interactTextTr.gameObject.activeSelf)
            {
                SetInteractTextParentToPlayer();
            }
        }
    }

    private void SetInteractTextTransform(Transform parent, string text = "")
    {
        // Null 체크 추가
        if(interactTextTr == null || parent == null || parent.gameObject == null)
        {
            if(interactTextTr != null) interactTextTr.gameObject.SetActive(false); // 숨기기
            return;
        }
        interactTextTr.SetParent(parent,false);
        interactTextTr.localPosition = Vector3.zero;
        interactText.text = text;
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            Gizmos.color = Color.yellow;

            // 현재 뷰 모드에 따라 다른 방식으로 Gizmos를 그립니다.
            if(ViewManager.Instance.CurrentViewMode == ViewModeType.View3D)
            {
                Gizmos.DrawWireSphere(transform.position, interactableRange);
            }
            else
            {
                // 2D 모드에서는 박스 형태로 그리기
                Gizmos.DrawWireCube(col.bounds.center, size_2D * 2);
            }
        }
    }
    // 오브젝트가 유효한지 확인하는 메서드
    private bool IsValidInteractable(IInteractable interactable)
    {
        var mb = interactable as MonoBehaviour;
        return mb != null && mb.gameObject != null && mb.gameObject.activeInHierarchy;
    }
    
    private void CloseNPCRelatedUIs()
    {
        if(!UIManager.HasInstance) return;

        // 상점 닫기
        if (UIManager.Instance.TryGetUI<ShopUI>(out var shopUI) && shopUI.gameObject.activeSelf)
        {
            shopUI.Close();
        }

        // 강화창 닫기
        if (UIManager.Instance.TryGetUI<EnhanceBoard>(out var enhanceUI) && enhanceUI.gameObject.activeSelf)
        {
            enhanceUI.Close();
        }

        // 치료창 닫기
        if (UIManager.Instance.TryGetUI<HealUI>(out var healUI) && healUI.gameObject.activeSelf)
        {
            healUI.Close();
        }
    }
}
