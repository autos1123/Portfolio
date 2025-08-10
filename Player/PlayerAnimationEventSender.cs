using UnityEngine;

public class PlayerAnimationEventSender:AnimationEventSender<PlayerController>
{
    public override void SendEvent()
    {
        originObject.Attack();
        SoundManager.Instance.PlaySFX(this.transform.position, SoundAddressbleName.HandAttack);
    }
    public void SendSkillEvent()
    {
        originObject.OnSkillInput?.Invoke();
        SoundManager.Instance.PlaySFX(this.transform.position, SoundAddressbleName.CastSound);
    }
}
