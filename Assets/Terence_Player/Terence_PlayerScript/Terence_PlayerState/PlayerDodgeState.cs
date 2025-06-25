using System;
using Unity.Android.Gradle.Manifest;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerDodgeState : PlayerBaseState
{
    private Vector3 dodgeDirection;
    private float dodgeTimer;
    private float dodgeStaminaCost; // Cache the stamina cost

    public PlayerDodgeState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        Debug.Log("Enter Dodging state");

        // --- Dodge Conditions & Stamina Check ---
        dodgeStaminaCost = context.staminaSystem.dodgeStaminaCost;
        if (!context.staminaSystem.CanSpendStamina(dodgeStaminaCost))
        {
            Debug.Log("Not enough stamina to dodge. Returning to Idle.");
            context.SwitchState(context.idleState);
            return; // Exit early if conditions aren't met
        }

        // Spend stamina immediately upon entering the dodge state
        context.staminaSystem.SpendStamina(dodgeStaminaCost);

        // Animation & Root Motion
        context.animator.SetBool("IsRolling", true);
        // Crucially, enable root motion ONLY if your dodge animation handles the full movement.
        // If not, you'll need to manually move the character in UpdateState.
        context.animator.applyRootMotion = true;

        DodgeInitialization();
    }

    public override void UpdateState()
    {
        // This UpdateState mainly monitors the dodge duration and transitions out.
        dodgeTimer += Time.deltaTime;

        // If your dodge animation does NOT use root motion for movement,
        // you would uncomment and use this:
        // context.Motor.Move(dodgeDirection * context.rollingSpeed, context.rollingSpeed);
        // Note: The PlayerMotor.Move expects a Vector2 for input, and then a speed.
        // If applying raw dodgeDirection with a speed here, you'd need a different Motor method
        // or convert dodgeDirection to a Vector2 and pass rollingSpeed.
        // Given root motion is usually preferred for dodges for precise animation control,
        // sticking with it for movement is often best.

        // Transition out of dodge state after duration
        if (dodgeTimer >= context.rollingDuration)
        {
            // First, check if the player is still grounded before transitioning.
            // A dodge might end mid-air if done near a ledge.
            if (!context.Motor.IsGrounded())
            {
                context.SwitchState(context.fallingState);
            }
            else if (context.inputHandler.GetMoveInput().magnitude > 0.1f)
            {
                context.SwitchState(context.movementState);
            }
            else
            {
                context.SwitchState(context.idleState);
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exit Dodging state");
        context.animator.SetBool("IsRolling", false);
        // Always disable root motion on exit unless the next state specifically needs it.
        // It's safer to have each state explicitly manage root motion.
        context.animator.applyRootMotion = false;

        dodgeTimer = 0f; // Reset timer for next dodge
    }

    private void DodgeInitialization()
    {
        // Get initial input direction from the PlayerInputHandler
        Vector2 rawMoveInput = context.inputHandler.GetMoveInput();
        Vector3 initialInputDirection = new Vector3(rawMoveInput.x, 0f, rawMoveInput.y);

        if (initialInputDirection.magnitude > 0.001f)
        {
            // Calculate dodge direction relative to camera
            // Use context.Motor's internal camera reference if possible, or pass it.
            // For now, assuming context.cameraTransform is available from PlayerStateMachine
            Vector3 camForward = context.Motor._cameraTransform.forward; // Access via Motor
            Vector3 camRight = context.Motor._cameraTransform.right; // Access via Motor
            camForward.y = 0; // Flatten
            camRight.y = 0;    // Flatten
            camForward.Normalize();
            camRight.Normalize();

            dodgeDirection = (camRight * initialInputDirection.x + camForward * initialInputDirection.z).normalized;

            Debug.Log($"Raw Move Input: {rawMoveInput}");
            Debug.Log($"Initial Input Direction: {initialInputDirection}");
            Debug.Log($"CamForward (flattened): {camForward}");
            Debug.Log($"CamRight (flattened): {camRight}");
            Debug.Log($"Calculated Dodge Direction: {dodgeDirection}");

            Quaternion targetRotation = Quaternion.LookRotation(dodgeDirection);
            context.transform.rotation = targetRotation;
        }
        else
        {
            // If no movement input, dodge in the direction the character is currently facing
            dodgeDirection = context.transform.forward;
            dodgeDirection.y = 0f; // Ensure it's horizontal
            dodgeDirection.Normalize();
        }
    }
}