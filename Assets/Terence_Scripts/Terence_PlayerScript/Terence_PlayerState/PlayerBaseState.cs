using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerStateMachine context; // Make it protected so derived states can access it

    // Constructor to receive the PlayerStateMachine context
    public PlayerBaseState(PlayerStateMachine currentContext)
    {
        context = currentContext;
    }

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();
}

