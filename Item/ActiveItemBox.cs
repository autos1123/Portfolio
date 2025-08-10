using UnityEngine;

public class ActiveItemBox:MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "[F] 줍기";
    [SerializeField] private Transform promptPivot;

    // 드랍 아이템 후보 ID 배열 (필요시 인스펙터에서 추가 가능)
    [SerializeField] private int[] possibleItemIds = { 4000, 4001, 4002, 4003 };
    // 한번 뽑히면 저장되는 아이템 ID 값
    private int? fixedPickedId; 
    public string InteractionPrompt => interactionPrompt;
    public Transform PromptPivot => promptPivot;

    public bool CanInteract(GameObject interactor)
    {
        // 플레이어(Inventory)만 상호작용 가능하도록 체크
        return interactor.GetComponent<Inventory>() != null;
    }

    public void Interact(GameObject interactor)
    {
        // 1. Inventory 컴포넌트 찾기
        var inventory = interactor.GetComponent<Inventory>();
        if(inventory == null)
        {
            Debug.LogError("[ActiveItemBox] 인터랙터에 Inventory 컴포넌트가 없습니다!");
            return;
        }

        // 2. 랜덤 아이템 선택 (한번상호작용하면 고정)
        if(possibleItemIds == null || possibleItemIds.Length == 0)
        {
            Debug.LogError("[ActiveItemBox] possibleItemIds 배열이 비어 있습니다!");
            return;
        }
        
        if (UIManager.Instance.IsUIOpen<TakeActiveItem>()) return;
        
        if(fixedPickedId == null) // 아직 아이템이 선택되지 않았다면
        {
            int randomIndex = Random.Range(0, possibleItemIds.Length);
            fixedPickedId = possibleItemIds[randomIndex];
        }
        int pickedId = fixedPickedId.Value;
        var acquiredItem = TableManager.Instance.GetTable<ActiveItemDataTable>().GetDataByID(pickedId);
        if(acquiredItem == null)
        {
            Debug.LogError($"[ActiveItemBox] 아이템 테이블에 ID {pickedId}가 없습니다!");
            return;
        }

        // 3. 액티브 슬롯 배열
        var activeItemSlots = inventory.activeItemSlots?.ToArray();
        if(activeItemSlots == null || activeItemSlots.Length == 0)
        {
            Debug.LogError("[ActiveItemBox] Inventory에 액티브 아이템 슬롯이 없습니다!");
            return;
        }

        // 4. UIManager로 UI 열기
        var uiManager = UIManager.Instance;
        if(uiManager == null)
        {
            Debug.LogError("[ActiveItemBox] UIManager.Instance가 null입니다!");
            return;
        }

        var takeActiveItemUI = uiManager.GetUI<TakeActiveItem>();
        if(takeActiveItemUI == null)
        {
            Debug.LogError("[ActiveItemBox] TakeActiveItem UI 프리팹을 찾을 수 없습니다!");
            return;
        }

        takeActiveItemUI.Open(activeItemSlots, acquiredItem, inventory,this);

        // ※ 필요에 따라 박스를 파괴하거나, 재사용금지 처리 등 추가 가능 -> TakeActiveItem.cs에서 처리햇읍니다 -lym
        InteractionController interactionController = interactor.GetComponent<InteractionController>();
        if (interactionController != null)
        {
            interactionController.SetInteractTextParentToPlayer(); // UI 숨기기 및 부모 리셋
        }
    }
}
