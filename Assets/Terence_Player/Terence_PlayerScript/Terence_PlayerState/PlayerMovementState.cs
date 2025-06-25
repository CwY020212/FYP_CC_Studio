using UnityEngine;

public class PlayerMovementState : PlayerBaseState
{
    private float smoothAnimXVelocity;
    private float smoothAnimYVelocity;
    public float smoothTime = 0.1f;

    public PlayerMovementState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        Debug.Log("Enter Moving State (Walking)");
        context.animator.SetBool("IsMoving", true);
        context.animator.SetBool("IsRunning", false);
    }

    public override void UpdateState()
    {
        // Check for grounded status using the Motor
        if (!context.Motor.IsGrounded())
        {
            context.SwitchState(context.fallingState);
            return;
        }

        Vector2 moveInput = context.inputHandler.GetMoveInput();

        // Pass movement input and walk speed to the PlayerMotor
        context.Motor.Move(moveInput, context.walkSpeed);

        HandleMovementAnimationParameters();

        // Run Input (If run is toggled on AND there's movement)
        if (context.inputHandler.GetRunInputHeld() && moveInput.magnitude > 0.1f)
        {
            context.SwitchState(context.runState);
            return;
        }

        // No Movement Input (Transition to Idle)
        if (moveInput.magnitude <= 0.1f)
        {
            context.inputHandler.DisableRunToggle(); // Turn off run toggle if player stops moving
            context.SwitchState(context.idleState);
            return;
        }

        // Dodge Input (High Priority)
        if (context.inputHandler.GetDodgeInputDown())
        {
            context.inputHandler.DisableRunToggle(); // Turn off run toggle if dodging
            context.SwitchState(context.dodgeState);
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
        Debug.Log("Exit Moving State");
        context.animator.SetBool("IsMoving", false);
        // When exiting, ensure animation parameters are reset or smoothly dampened to zero if needed
        context.animator.SetFloat("WalkX", 0f);
        context.animator.SetFloat("WalkY", 0f);
    }

    private void HandleMovementAnimationParameters()
    {
        Vector2 moveInput = context.inputHandler.GetMoveInput();
        float targetAnimX = moveInput.x;
        float targetAnimY = moveInput.y;
        context.animator.SetFloat("WalkX", Mathf.SmoothDamp(context.animator.GetFloat("WalkX"), targetAnimX, ref smoothAnimXVelocity, smoothTime));
        context.animator.SetFloat("WalkY", Mathf.SmoothDamp(context.animator.GetFloat("WalkY"), targetAnimY, ref smoothAnimYVelocity, smoothTime));
    }
}
