using UnityEngine;

public abstract class AnimationEventSender<T> : MonoBehaviour
{
    protected T originObject;

    private void Start()
    {
        originObject = transform.parent.GetComponent<T>();
    }

    public abstract void SendEvent();
}
