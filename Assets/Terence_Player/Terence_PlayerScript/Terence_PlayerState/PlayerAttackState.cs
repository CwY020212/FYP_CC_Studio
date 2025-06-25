using UnityEngine;

public class PlayerAttackState :PlayerBaseState
{
    public enum AttackType
    {
        None,
        LightAttack,
        HeavyAttack,
        UltimateAttack,
        SkillAttack
    }

    // NEW: Determines which hand's weapon is the primary for this attack instance
    public PlayerWeaponManager.WeaponHand attackHand = PlayerWeaponManager.WeaponHand.RightHand;

    private AttackType currentAttackType = AttackType.None;
    private PlayerWeaponSO activeWeaponSO; // The SO for the weapon performing this specific attack
    private bool isAttackActive = false; // Flag to track if an attack animation is currently playing

    // Cooldown tracking for skill and ultimate attacks
    private float lastSkillAttackTime = -Mathf.Infinity;
    private float lastUltimateAttackTime = -Mathf.Infinity;

    public PlayerAttackState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        Debug.Log("Enter Attack state");

        // Determine the active weapon based on the 'attackHand' parameter
        activeWeaponSO = (attackHand == PlayerWeaponManager.WeaponHand.RightHand) ?
                         context.weaponManager.CurrentRightHandWeapon :
                         context.weaponManager.CurrentLeftHandWeapon;

        // --- Attack Conditions (Algorithm for entering the attack) ---
        if (currentAttackType == AttackType.None)
        {
            Debug.LogWarning("PlayerAttackState entered with no specific attack type. Returning to idle.");
            context.SwitchState(context.idleState);
            return;
        }

        // IMPORTANT CHANGE HERE:
        // A weapon is considered "valid" for an attack if an activeWeaponSO exists.
        // It does NOT necessarily need a weaponPrefab if it's a brawler/unarmed attack.
        if (activeWeaponSO == null) // Only check if the SO itself is null
        {
            Debug.LogWarning($"Player tried to attack with {attackHand} but no weapon SO is assigned! Returning to idle.");
            context.SwitchState(context.idleState);
            return;
        }
        // If the activeWeaponSO is the defaultBrawlerWeapon (or defaultOffHandWeapon and it's also brawler),
        // we consider it valid even if it has no weaponPrefab.
        // The check for activeWeaponSO.weaponPrefab should only be relevant if you're trying to EQUIP a model,
        // which the PlayerWeaponManager already handles. For attacking, the SO itself is enough.


        // Prevent starting a new attack if one is already playing.
        if (isAttackActive)
        {
            Debug.Log("Attack animation is already active, cannot start new attack. Consider queuing for combo.");
            context.SwitchState(context.idleState);
            return;
        }

        // --- Stamina Check ---
        float staminaCost = GetStaminaCostForAttack(currentAttackType);
        if (context.staminaSystem != null && !context.staminaSystem.CanSpendStamina(staminaCost))
        {
            Debug.Log($"Not enough stamina ({context.staminaSystem.currentStamina:F2}) to perform {currentAttackType.ToString()} (cost: {staminaCost:F2}). Returning to idle.");
            context.SwitchState(context.idleState);
            return;
        }

        // --- Cooldown Check for Skill and Ultimate Attacks ---
        if (currentAttackType == AttackType.SkillAttack)
        {
            if (Time.time < lastSkillAttackTime + activeWeaponSO.skillAttackCooldown)
            {
                Debug.Log($"Skill Attack is on cooldown. Time remaining: {(lastSkillAttackTime + activeWeaponSO.skillAttackCooldown) - Time.time:F2}s");
                context.SwitchState(context.idleState);
                return;
            }
        }
        else if (currentAttackType == AttackType.UltimateAttack)
        {
            if (Time.time < lastUltimateAttackTime + activeWeaponSO.ultimateAttackCooldown)
            {
                Debug.Log($"Ultimate Attack is on cooldown. Time remaining: {(lastUltimateAttackTime + activeWeaponSO.ultimateAttackCooldown) - Time.time:F2}s");
                context.SwitchState(context.idleState);
                return;
            }
        }

        // If all conditions pass, proceed with the attack
        Debug.Log($"Performing {currentAttackType.ToString()} with {activeWeaponSO.weaponName} in {attackHand}!");
        PlayAttackAnimation();
        isAttackActive = true; // Mark attack as active

        // Consume stamina and record cooldowns
        context.staminaSystem?.SpendStamina(staminaCost);
        if (currentAttackType == AttackType.SkillAttack)
        {
            lastSkillAttackTime = Time.time;
        }
        else if (currentAttackType == AttackType.UltimateAttack)
        {
            lastUltimateAttackTime = Time.time;
        }

        context.Motor.enabled = false; // Disable PlayerMotor during attack
    }


    public override void UpdateState()
    {
        // This state's duration is primarily managed by animation events (OnAttackAnimationEnd).
        // If the attack animation handles character movement via root motion,
        // then Motor.Move() should NOT be called from here.
        // If it doesn't, and you want a lunge or dash, you might apply force using context.Motor.Move()
        // For example: context.Motor.Move(context.transform.forward * activeWeaponSO.attackMovementSpeed * Time.deltaTime);

        // Crucial: Check for landing while in attack state if it's possible to fall off a ledge.
        if (!context.Motor.IsGrounded())
        {
            Debug.Log("Fell off ledge during attack. Transitioning to falling state.");
            context.SwitchState(context.fallingState);
            return;
        }
    }

    public override void ExitState()
    {
        // Re-enable the PlayerMotor
        context.Motor.enabled = true;

        // Reset animation triggers to ensure a clean exit
        context.animator.ResetTrigger("LightAttack"); // Assuming these are trigger parameters
        context.animator.ResetTrigger("HeavyAttack");
        context.animator.ResetTrigger("SkillAttack");
        context.animator.ResetTrigger("UltimateAttack");

        currentAttackType = AttackType.None; // Reset attack type for next time
        isAttackActive = false;               // Reset active flag
        activeWeaponSO = null;                // Clear active weapon reference

        Debug.Log("Exit Attack state");
    }

    /// <summary>
    /// Public method for PlayerStateMachine or PlayerInputHandler to set the desired attack type
    /// and the hand from which the attack originates.
    /// </summary>
    /// <param name="type">The type of attack to perform.</param>
    /// <param name="hand">The hand (Left/Right) from which the attack should originate.</param>
    public void SetAttackParameters(AttackType type, PlayerWeaponManager.WeaponHand hand = PlayerWeaponManager.WeaponHand.RightHand)
    {
        currentAttackType = type;
        attackHand = hand;
    }

    // Helper to get stamina cost from the active weapon SO
    private float GetStaminaCostForAttack(AttackType type)
    {
        if (activeWeaponSO == null) return 0f;

        switch (type)
        {
            case AttackType.LightAttack: return activeWeaponSO.lightAttackStaminaCost;
            case AttackType.HeavyAttack: return activeWeaponSO.heavyAttackStaminaCost;
            case AttackType.SkillAttack: return activeWeaponSO.skillAttackStaminaCost;
            case AttackType.UltimateAttack: return activeWeaponSO.ultimateAttackStaminaCost;
            default: return 0f;
        }
    }

    private void PlayAttackAnimation()
    {
        if (activeWeaponSO == null) return;

        // Use CrossFade for smoother transitions based on animation clip name
        // Ensure your PlayerWeaponSO has AnimationClip fields linked in the Inspector
        switch (currentAttackType)
        {
            case AttackType.LightAttack:
                // Consider having separate animations for left/right hand attacks
                // e.g., activeWeaponSO.GetLightAttackClip(attackHand)
                if (activeWeaponSO.lightAttack != null) context.animator.CrossFade(activeWeaponSO.lightAttack.name, 0.1f);
                else Debug.LogWarning($"No Light Attack animation assigned for {activeWeaponSO.weaponName}.");
                break;
            case AttackType.HeavyAttack:
                if (activeWeaponSO.heavyAttack != null) context.animator.CrossFade(activeWeaponSO.heavyAttack.name, 0.1f);
                else Debug.LogWarning($"No Heavy Attack animation assigned for {activeWeaponSO.weaponName}.");
                break;
            case AttackType.SkillAttack:
                if (activeWeaponSO.skillAttack != null) context.animator.CrossFade(activeWeaponSO.skillAttack.name, 0.1f);
                else Debug.LogWarning($"No Skill Attack animation assigned for {activeWeaponSO.weaponName}.");
                break;
            case AttackType.UltimateAttack:
                if (activeWeaponSO.ultimateAttack != null) context.animator.CrossFade(activeWeaponSO.ultimateAttack.name, 0.1f);
                else Debug.LogWarning($"No Ultimate Attack animation assigned for {activeWeaponSO.weaponName}.");
                break;
            default:
                Debug.LogWarning("Attempted to play unknown attack animation type: " + currentAttackType);
                break;
        }
    }

    // This method is called from PlayerStateMachine's OnAttackAnimationEnd via an Animation Event
    public void OnAttackAnimationEnd()
    {
        Debug.Log("Attack animation ended. Transitioning to appropriate state.");
        // If a combo input was detected during the attack, you might transition to another attack state
        // or a combo handling state here instead of directly to idle/movement.

        if (context.inputHandler.GetMoveInput().magnitude > 0.1f)
        {
            context.SwitchState(context.movementState);
        }
        else
        {
            context.SwitchState(context.idleState);
        }
    }
}
