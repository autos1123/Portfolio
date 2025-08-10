using UnityEngine;

public class PlayerAnimationData
{
    private string idleParameterName = "Idle";
    private string moveParameterName = "isMoving";
    private string dashParameterName = "Dash";
    private string jumpParameterName = "isJumping";
    private string attack1ParameterName = "Attack1";
    private string attack2ParameterName = "Attack2";
    private string attack3ParameterName = "Attack3";
    private string skillParameterName = "Skill";
    private string deadParameterName = "isDead";
    private string knockBackParameterName = "isKnockBack";
    private string fallParameterName = "isFalling";
    private string landingParameterName = "Landing";

    public int IdleParameterHash { get; private set; }
    public int MoveParameterHash { get; private set; }
    public int DashParameterHash { get; private set; }
    public int JumpParameterHash { get; private set; }
    public int Attack1ParameterHash { get; private set; }
    public int Attack2ParameterHash { get; private set; }
    public int Attack3ParameterHash { get; private set; }
    public int SkillParameterHash { get; private set; }
    public int DeadParameterHash { get; private set; }
    public int KnockBackParameterHash { get; private set; }
    public int FallParameterHash { get; private set; }
    public int LandingParameterHash { get; private set; }

    public PlayerAnimationData()
    {
        IdleParameterHash = Animator.StringToHash(idleParameterName);
        MoveParameterHash = Animator.StringToHash(moveParameterName);
        DashParameterHash = Animator.StringToHash(dashParameterName);
        JumpParameterHash = Animator.StringToHash(jumpParameterName);
        Attack1ParameterHash = Animator.StringToHash(attack1ParameterName);
        Attack2ParameterHash = Animator.StringToHash(attack2ParameterName);
        Attack3ParameterHash = Animator.StringToHash(attack3ParameterName);
        SkillParameterHash = Animator.StringToHash(skillParameterName);
        DeadParameterHash = Animator.StringToHash(deadParameterName);
        KnockBackParameterHash = Animator.StringToHash(knockBackParameterName);
        FallParameterHash = Animator.StringToHash(fallParameterName);
        LandingParameterHash = Animator.StringToHash(landingParameterName);
    }
}
