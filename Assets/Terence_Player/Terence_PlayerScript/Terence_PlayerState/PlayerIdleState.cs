using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine context) : base(context) { }

    public override void EnterState()
    {
        Debug.Log("Enter Idle State");
        // CrossFade to the base idle animation
        context.animator.CrossFade("Terence_PlayerIdle", 0.1f);
    }

    public override void UpdateState()
    {
        // --- Transition Checks ---

        // Check for falling: This is a high-priority check, always put it first if applicable.
        if (!context.Motor.IsGrounded())
        {
            context.SwitchState(context.fallingState);
            return;
        }

        // Check for dodge input
        if (context.inputHandler.GetDodgeInputDown())
        {
            context.SwitchState(context.dodgeState);
            return;
        }

        // Check for attack input (Light Attack)
        // Now using SetAttackParameters to specify the hand.
        // Assuming GetLightAttackInputDown() triggers a primary hand attack (e.g., right hand)
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

        // Check for movement input
        if (context.inputHandler.GetMoveInput().magnitude > 0.1f)
        {
            if (context.inputHandler.GetRunInputHeld())
            {
                context.SwitchState(context.runState);
            }
            else
            {
                context.SwitchState(context.movementState);
            }
            return;
        }

        // Note: Interaction input is handled by PlayerInteractionController in its Update,
        // which then triggers a state switch to PlayerInteractState. No need to duplicate here.
    }

    public override void ExitState()
    {
        Debug.Log("Exit Idle State");
        // No specific cleanup needed for idle state on exit
    }
}
