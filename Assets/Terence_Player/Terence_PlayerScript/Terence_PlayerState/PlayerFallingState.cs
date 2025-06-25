using UnityEditor.Timeline.Actions;
using UnityEngine;

public class PlayerFallingState : PlayerBaseState
{
    public PlayerFallingState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        Debug.Log("Enter Falling State");
        context.animator.SetBool("IsFalling", true);
        // Ensure other movement related anim parameters are off for clean transitions
        context.animator.SetBool("IsMoving", false);
        context.animator.SetBool("IsRunning", false);
        context.animator.SetFloat("WalkX", 0f);
        context.animator.SetFloat("WalkY", 0f);
        context.animator.SetFloat("RunX", 0f);
        context.animator.SetFloat("RunY", 0f);
    }

    public override void UpdateState()
    {
        // Continuously check if the player has landed using PlayerMotor's IsGrounded.
        if (context.Motor.IsGrounded()) // Changed from context.IsGrounded() to context.Motor.IsGrounded()
        {
            // Transition based on current movement input upon landing.
            if (context.inputHandler.GetMoveInput().magnitude > 0.1f)
            {
                context.SwitchState(context.movementState);
                return;
            }
            else
            {
                context.SwitchState(context.idleState);
                return;
            }
        }

        // Allow for air control (movement while falling)
        // If your PlayerMotor's FixedUpdate handles vertical movement,
        // this part is primarily for animating air movement.
        Vector2 moveInput = context.inputHandler.GetMoveInput();
        // You might need to adjust the speed or use different blend tree parameters for air control.
        // Here, we just update the animation parameters. The actual movement in air is still handled by Motor's gravity.
        context.animator.SetFloat("WalkX", moveInput.x);
        context.animator.SetFloat("WalkY", moveInput.y);
    }

    public override void ExitState()
    {
        Debug.Log("Exit Falling State");
        context.animator.SetBool("IsFalling", false);
    }
}
