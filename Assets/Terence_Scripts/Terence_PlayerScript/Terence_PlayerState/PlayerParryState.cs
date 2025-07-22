using UnityEngine;

public class PlayerParryState : PlayerBaseState
{
    private float parryWindow = 0.5f;
    private float parryTimer = 0f;
    public PlayerParryState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        //context.animator.SetBool("IsParrying", true);
        parryTimer = parryWindow;
    }

    public override void UpdateState()
    {
        parryTimer -= Time.deltaTime;
        if (parryTimer <= 0f)
        {
            context.SwitchState(context.idleState); // Return to idle
            return;
        }
    }

    public override void ExitState()
    {
        // context.animator.SetBool("IsParrying", false);
    }
}
