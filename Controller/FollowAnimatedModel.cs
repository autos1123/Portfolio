using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAnimatedModel:MonoBehaviour
{
    public Transform modelTransform; // Character_Viking_Black
    private Vector3 previousModelPosition;

    void Start()
    {
        previousModelPosition = modelTransform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = modelTransform.position - previousModelPosition;

        // 부모 오브젝트를 이동
        transform.position += delta;

        // 모델 위치는 상대적으로 되돌림
        modelTransform.position -= delta;

        previousModelPosition = modelTransform.position;
    }
}
