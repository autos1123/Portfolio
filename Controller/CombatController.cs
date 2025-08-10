using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour, IDamagable
{
    private BaseController baseController;
    private Transform visualTransform;

    private float attackAngle = 100f; // 공격 각도

    private void Start()
    {
        baseController = GetComponent<BaseController>();
        visualTransform = baseController.VisualTransform;
    }

    /// <summary>
    /// 공격 범위 내에 있는 타겟 콜라이더를 반환합니다.
    /// </summary>
    /// <param name="layer">타겟 레이어</param>
    /// <returns></returns>
    public virtual Collider[] GetTargetColliders(LayerMask layer)
    {
        float attackRange = baseController.Condition.GetTotalCurrentValue(ConditionType.AttackRange);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, layer);
        List<Collider> filteredColliders = new List<Collider>();

        foreach(Collider collider in hitColliders)
        {
            Vector3 directionToTarget = (collider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(visualTransform.forward, directionToTarget);

            if(angle <= attackAngle)
                filteredColliders.Add(collider);
        }
        return filteredColliders.ToArray();
    }

    public virtual bool GetDamaged(float damage)
    {
        // Knockback();

        if(baseController.Condition.IsDied)
            return false; // 이미 죽은 상태에서는 데미지를 받지 않음

        float defense = baseController.Condition.GetTotalCurrentValue(ConditionType.Defense);
        float reducedDamage = Mathf.Max(5, damage - defense);

        SoundManager.Instance.PlaySFX(transform.position, SoundAddressbleName.HitSound);

        if(baseController.Condition.GetDamaged(reducedDamage))
        {
            baseController.Die();
            return false;
        }

        baseController.Hit();
        return true;
    }

    public virtual bool Heal(float Heal)
    {
        baseController.Condition.Heal(Heal);
        return true;
    }

    public Vector3 GetDamagedPos()
    {
        Collider col = GetComponent<Collider>();
        if(col != null)
        {
            return col.bounds.center + (col.transform.up * (col.bounds.size.y / 2)); // 충돌체의 중심 위치 반환
        }
        return transform.position;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if(Application.isPlaying && baseController.IsInitialized)
        {
            float attackRange = baseController.Condition.GetTotalCurrentValue(ConditionType.AttackRange);

            // 공격 범위를 시각적으로 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.black;
            float angle1 = -attackAngle / 2f;
            float angle2 = attackAngle / 2f;

            Vector3 dir1 = Quaternion.Euler(0, angle1, 0) * visualTransform.forward;
            Vector3 dir2 = Quaternion.Euler(0, angle2, 0) * visualTransform.forward;

            Vector3 point1 = transform.position + dir1 * attackRange;
            Vector3 point2 = transform.position + dir2 * attackRange;

            Gizmos.DrawLine(transform.position, point1);
            Gizmos.DrawLine(transform.position, point2);
        }
    }
}
