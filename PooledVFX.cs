using UnityEngine;

public class PooledVFX:MonoBehaviour, IPoolObject
{
    [SerializeField] private PoolType poolType = PoolType.Zone; // Inspector에서 지정
    [SerializeField] private int poolSize = 10; // Inspector에서 지정

    public PoolType PoolType => poolType;
    public int PoolSize => poolSize;
    public GameObject GameObject => gameObject;

    public void PlayVFX(float duration)
    {
        var ps = GetComponent<ParticleSystem>();
        ps?.Play();
        PoolManager.Instance.ReturnObject(this, duration);
    }
}
