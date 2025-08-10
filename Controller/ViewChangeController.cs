using UnityEngine;

public class ViewChangeController : MonoBehaviour
{
    private Rigidbody _Rigidbody;
    private BoxCollider col;

    // 캐릭터 충돌체 변환 관련
    protected Vector3 colliderSizeTmp;
    [SerializeField] private float fullSize;

    protected virtual void OnEnable()
    {
        ViewManager.Instance.OnViewChanged += OnViewChange;
    }

    protected virtual void OnDisable()
    {
        if(ViewManager.HasInstance)
            ViewManager.Instance.OnViewChanged -= OnViewChange;
    }

    private void Start()
    {
        Init();
        OnViewChange(ViewManager.Instance.CurrentViewMode);
    }

    private void Init()
    {
        _Rigidbody = GetComponent<Rigidbody>();
        col = GetComponent<BoxCollider>();
        colliderSizeTmp = col.size;
    }

    public virtual void OnViewChange(ViewModeType viewMode)
    {
        if(_Rigidbody == null || col == null)
        {
            Init();
        }

        if(viewMode == ViewModeType.View2D)
        {
            _Rigidbody.detectCollisions = false;

            // 콜라이더 사이즈 확장
            colliderSizeTmp = col.size;
            col.center = new Vector3(transform.position.z, col.center.y, 0); // 충돌체 중심점 설정
            col.size = new Vector3(fullSize, colliderSizeTmp.y, colliderSizeTmp.z); // 충돌체 사이즈 확장

            // 겹치는 오브젝트가 있을 경우 콜라이더 위치 조정
            SetPositionWhenViewChanged();

            _Rigidbody.detectCollisions = true;
        }
        else if(viewMode == ViewModeType.View3D)
        {
            col.center = new Vector3(0, col.center.y, 0); // 2D 모드에서는 콜라이더 중심을 약간 위로 이동
            col.size = colliderSizeTmp; // 3D 모드에서는 원래 크기로 복원
        }
    }

    /// <summary>
    /// 2D 모드에서 뷰가 변경될 때 콜라이더의 위치를 조정합니다.
    /// </summary>
    /// <param name="col"></param>
    private void SetPositionWhenViewChanged()
    {
        // 현재 콜라이더의 경계 박스에서 충돌한 오브젝트들을 가져옴
        Vector3 center = transform.position;
        center.z = 0;
        Collider[] hit = Physics.OverlapBox(center,
                                            col.bounds.extents,
                                            Quaternion.identity,
                                            LayerMask.GetMask("Obstacle"),
                                            QueryTriggerInteraction.Ignore);

        if(hit.Length == 0) return;

        // 충돌한 콜라이더의 최대 x좌표와 최소 x좌표를 계산
        float maxX = hit[0].bounds.max.x;
        float minX = hit[0].bounds.min.x;
        for(int i = 1; i < hit.Length; i++)
        {
            if(hit[i].bounds.max.x > maxX)
            {
                maxX = hit[i].bounds.max.x;
            }
            if(hit[i].bounds.min.x < minX)
            {
                minX = hit[i].bounds.min.x;
            }
        }

        // 충돌한 콜라이더(col)의 왼쪽 경계(min.x)로부터 현재 위치까지의 x 거리 계산(+여유값 0.5f 추가)
        float distance = transform.position.x - minX + 0.5f;
        Vector3 direction = Vector3.left;

        if(gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // 적 캐릭터는 오른쪽으로 이동
            distance = maxX - transform.position.x + 0.5f;
            direction = Vector3.right;
        }

        // 현재 오브젝트를 왼쪽으로 distance만큼 이동시켜, 콜라이더에서 완전히 벗어나게 함
        transform.position = transform.position + direction * distance;

        SetPositionWhenViewChanged();
    }
}
