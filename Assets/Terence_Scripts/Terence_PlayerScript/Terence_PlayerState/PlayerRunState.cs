using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    private float smoothAnimXVelocity;
    private float smoothAnimYVelocity;
    public float smoothTime = 0.1f; // Adjust this value for animation blending

    private float noMovementTimer = 0f;
    public float noMovementThreshold = 0.75f; // Time before transitioning to idle if no movement input while running

    public PlayerRunState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        Debug.Log("Enter Running State");
        context.animator.SetBool("IsRunning", true);
        context.animator.SetBool("IsMoving", true); // Running is also a form of moving
        noMovementTimer = 0f;
    }

    public override void UpdateState()
    {
        // Check for grounded status using the Motor
        if (!context.Motor.IsGrounded())
        {
            context.SwitchState(context.fallingState);
            return;
        }

        // Continuously drain stamina while in the run state
        context.staminaSystem.DrainStaminaOverTime(context.staminaSystem.sprintStaminaDrainRate);

        // Check for stamina depletion
        if (context.staminaSystem.currentStamina <= 0.01f)
        {
            Debug.Log("Stamina depleted, automatically exiting run state.");
            context.inputHandler.DisableRunToggle(); // Turn off the toggle
            context.SwitchState(context.idleState); // Or movementState, depending on desired behavior
            return;
        }

        Vector2 moveInput = context.inputHandler.GetMoveInput();

        // Pass movement input and run speed to the PlayerMotor
        context.Motor.Move(moveInput, context.runSpeed);

        HandleMovementAnimationParameters();

        // Check if movement input has stopped while in run state
        if (moveInput.magnitude <= 0.01f)
        {
            noMovementTimer += Time.deltaTime;
            if (noMovementTimer >= noMovementThreshold)
            {
                Debug.Log("No movement input, exiting run state.");
                context.inputHandler.DisableRunToggle(); // Turn off the toggle
                context.SwitchState(context.idleState);
                return;
            }
        }
        else
        {
            noMovementTimer = 0f; // Reset timer if movement input is detected
        }

        // Transitions: Dodge input (High Priority)
        if (context.inputHandler.GetDodgeInputDown())
        {
            context.inputHandler.DisableRunToggle(); // Turn off the toggle if dodging
            context.SwitchState(context.dodgeState);
            return;
        }

        // If run input is released or toggled off, transition to movement state (or idle if no movement)
        if (!context.inputHandler.GetRunInputHeld())
        {
            if (moveInput.magnitude > 0.1f)
            {
                context.SwitchState(context.movementState);
            }
            else
            {
                context.SwitchState(context.idleState);
            }
            return;
        }

        if (context.inputHandler.GetLightAttackInputDown())
        {
            context.attackState.SetAttackParameters(PlayerAttackState.AttackType.LightAttack, PlayerWeaponManager.WeaponHand.RightHand);
            context.SwitchState(context.attackState);
            return;
        }

        if (context.inputHandler.GetHeavyAttackInputDown())
        {
            context.attackState.SetAttackParameters(PlayerAttackState.AttackType.HeavyAttack, PlayerWeaponManager.WeaponHand.RightHand);
            context.SwitchState(context.attackState);
            return;
        }
        if (context.inputHandler.GetSkillAttackInputDown())
        {
            context.attackState.SetAttackParameters(PlayerAttackState.AttackType.SkillAttack, PlayerWeaponManager.WeaponHand.RightHand);
            context.SwitchState(context.attackState);
            return;
        }
        if (context.inputHandler.GetUltimateAttackInputDown())
        {
            context.attackState.SetAttackParameters(PlayerAttackState.AttackType.UltimateAttack, PlayerWeaponManager.WeaponHand.RightHand);
            context.SwitchState(context.attackState);
            return;
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exit Running State");
        context.animator.SetBool("IsRunning", false);
        context.animator.SetBool("IsMoving", false); // Assuming running implies moving, so set this to false on exit
        noMovementTimer = 0f;
        // Reset run animation parameters
        context.animator.SetFloat("RunX", 0f);
        context.animator.SetFloat("RunY", 0f);
    }

    private void HandleMovementAnimationParameters()
    {
        Vector2 moveInput = context.inputHandler.GetMoveInput();
        float targetAnimX = moveInput.x;
        float targetAnimY = moveInput.y;
        context.animator.SetFloat("RunX", Mathf.SmoothDamp(context.animator.GetFloat("RunX"), targetAnimX, ref smoothAnimXVelocity, smoothTime));
        context.animator.SetFloat("RunY", Mathf.SmoothDamp(context.animator.GetFloat("RunY"), targetAnimY, ref smoothAnimYVelocity, smoothTime));
    }
}
