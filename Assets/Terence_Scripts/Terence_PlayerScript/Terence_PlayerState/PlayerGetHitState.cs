using UnityEngine;

public class PlayerGetHitState :PlayerBaseState
{
    private HitData currentHitData;
    private float hitStunTimer;

    // Animation trigger names (customize these to match your Animator)
    private const string ANIM_HIT_FRONT = "Hit_Front";
    private const string ANIM_HIT_BACK = "Hit_Back";
    private const string ANIM_HIT_LEFT = "Hit_Left";
    private const string ANIM_HIT_RIGHT = "Hit_Right";
    private const string ANIM_HIT_GENERIC = "Hit_Generic"; // Fallback for diagonal or undefined hits

    // For controlled knockback (if not using full ragdoll)
    private Vector3 currentControlledKnockbackVelocity;
    [Tooltip("Rate at which controlled knockback force dissipates.")]
    [SerializeField] // ADDED: Made serializable to adjust in Inspector
    private float knockbackDecelerationRate = 8f; // Adjust in Inspector for desired feel

    public PlayerGetHitState(PlayerStateMachine currentContext) : base(currentContext)
    {
        // The 'context' field is already set by the base constructor using 'currentContext'.
    }

    /// <summary>
    /// Sets the data for the hit before entering the state.
    /// This method is crucial as the HitData cannot be passed directly into the constructor.
    /// </summary>
    /// <param name="data">The HitData struct containing all relevant hit information.</param>
    public void SetHitData(HitData data)
    {
        currentHitData = data;
    }

    public override void EnterState()
    {
        Debug.Log("Enter Get Hit State");

        // 2. Apply damage to the player's HealthSystem
        if (context.healthSystem != null)
        {
            context.healthSystem.TakeDamage(currentHitData.damageAmount);
            Debug.Log($"Player took {currentHitData.damageAmount} damage. Current Health: {context.healthSystem.CurrentHealth}");

            // Check for death immediately after taking damage
            if (context.healthSystem.CurrentHealth <= 0)
            {
                Debug.Log("Player health dropped to zero. Transitioning to Death state (or handling death).");
                // TODO: Implement a DeathState and switch to it here if applicable.
                // context.SwitchState(context.deathState); // Once DeathState is implemented
                context.SwitchState(context.idleState); // As a temporary fallback
                return;
            }
        }
        else
        {
            Debug.LogWarning("PlayerStateMachine is missing a reference to HealthSystem. Damage not applied.");
        }

        // 3. Determine hit direction and play appropriate animation
        string hitAnimTrigger = GetHitAnimationTrigger(currentHitData.attackerPosition);
        if (!string.IsNullOrEmpty(hitAnimTrigger))
        {
            context.animator.SetTrigger(hitAnimTrigger);
            Debug.Log($"Playing hit animation: {hitAnimTrigger}");
        }
        else
        {
            // Fallback for unexpected or generic hit angles
            Debug.LogWarning("No specific hit animation trigger found for direction. Playing generic hit animation.");
            context.animator.SetTrigger(ANIM_HIT_GENERIC);
        }

        // 4. Handle Knockback or Full Ragdoll Activation
        if (currentHitData.activatesFullRagdoll && context.playerRagdollController != null)
        {
            Debug.Log("Activating full ragdoll from hit.");
            // Ensure PlayerRagdollController is responsible for disabling the CharacterController
            context.playerRagdollController.EnableRagdoll(currentHitData.initialRagdollImpulse, currentHitData.knockbackForce);
            // Ragdoll duration might be fixed or slightly longer than hit stun
            hitStunTimer = currentHitData.hitStunDuration > 0 ? currentHitData.hitStunDuration : 1.5f; // Default ragdoll time
        }
        else // Controlled knockback using CharacterController and animations
        {
            // Calculate initial knockback velocity for CharacterController
            Vector3 knockbackDir = (context.transform.position - currentHitData.attackerPosition).normalized;
            knockbackDir.y = 0; // Usually horizontal knockback, adjust if vertical knockback is desired
            knockbackDir.Normalize();
            currentControlledKnockbackVelocity = knockbackDir * currentHitData.knockbackForce;

            Debug.Log($"Applying controlled knockback: Direction={currentControlledKnockbackVelocity.normalized}, Force={currentHitData.knockbackForce}");
            hitStunTimer = currentHitData.hitStunDuration; // Use the provided hit stun duration
        }
    }

    public override void UpdateState()
    {
        // Decrement the hit stun timer
        hitStunTimer -= Time.deltaTime;

        // If not ragdolling, apply controlled knockback via CharacterController
        if (!currentHitData.activatesFullRagdoll)
        {
            // Apply horizontal knockback
            // The CharacterController should be enabled for this.
            context.characterController.Move(currentControlledKnockbackVelocity * Time.deltaTime);

            // Decelerate the knockback over time
            currentControlledKnockbackVelocity = Vector3.Lerp(currentControlledKnockbackVelocity, Vector3.zero, knockbackDecelerationRate * Time.deltaTime);
        }
        // If ragdolling, physics handles the movement, no need for CharacterController.Move here.

        // Check if the hit stun duration has ended
        if (hitStunTimer <= 0)
        {
            // If the hit animation has a specific exit point, you might wait for an animation event here.
            // For simplicity, we'll transition out immediately when the timer expires.
            TransitionAfterHit();
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exit Get Hit State");

        // Reset any animation triggers that were set
        context.animator.ResetTrigger(ANIM_HIT_FRONT);
        context.animator.ResetTrigger(ANIM_HIT_BACK);
        context.animator.ResetTrigger(ANIM_HIT_LEFT);
        context.animator.ResetTrigger(ANIM_HIT_RIGHT);
        context.animator.ResetTrigger(ANIM_HIT_GENERIC);

        // Disable full ragdoll if it was activated
        if (currentHitData.activatesFullRagdoll && context.playerRagdollController != null)
        {
            context.playerRagdollController.DisableRagdoll();
        }

        // Clear hit data and any remaining knockback velocity
        currentHitData = new HitData(); // Reset struct to default values
        currentControlledKnockbackVelocity = Vector3.zero;
    }

    /// <summary>
    /// Determines the appropriate hit animation trigger based on the attacker's position relative to the player.
    /// </summary>
    /// <param name="attackerPosition">The world position of the entity that hit the player.</param>
    /// <returns>The name of the animation trigger to set on the player's animator.</returns>
    private string GetHitAnimationTrigger(Vector3 attackerPosition)
    {
        if (attackerPosition == Vector3.zero) return ANIM_HIT_GENERIC; // Fallback if no attacker position

        // Calculate the direction from the player to the attacker, flattened to the horizontal plane
        Vector3 directionToAttacker = (attackerPosition - context.transform.position).normalized;
        directionToAttacker.y = 0; // Ignore vertical component for directional hits
        directionToAttacker.Normalize();

        // Get player's forward and right vectors, flattened
        Vector3 playerForward = context.transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        Vector3 playerRight = context.transform.right;
        playerRight.y = 0;
        playerRight.Normalize();

        // Use dot products to determine the relative direction
        // Dot product of two normalized vectors gives the cosine of the angle between them.
        // 1 means same direction, -1 means opposite direction, 0 means perpendicular.
        float dotForward = Vector3.Dot(playerForward, directionToAttacker);
        float dotRight = Vector3.Dot(playerRight, directionToAttacker);

        // Define thresholds for direction (adjust these based on your animation needs)
        // Values closer to 1 or -1 indicate a stronger alignment with that axis.
        const float THRESHOLD_DIRECTIONAL = 0.6f; // Adjust this threshold (e.g., 0.7 for stricter direction)

        if (dotForward > THRESHOLD_DIRECTIONAL)
        {
            return ANIM_HIT_FRONT;
        }
        else if (dotForward < -THRESHOLD_DIRECTIONAL)
        {
            return ANIM_HIT_BACK;
        }
        else if (dotRight > THRESHOLD_DIRECTIONAL)
        {
            return ANIM_HIT_RIGHT;
        }
        else if (dotRight < -THRESHOLD_DIRECTIONAL)
        {
            return ANIM_HIT_LEFT;
        }
        else
        {
            // If it's a diagonal hit or none of the main directions are strong enough
            return ANIM_HIT_GENERIC;
        }
    }

    /// <summary>
    /// Determines the next state for the player after recovering from the hit stun.
    /// </summary>
    private void TransitionAfterHit()
    {
        // First, check if the player is now dead.
        if (context.healthSystem != null && context.healthSystem.CurrentHealth <= 0)
        {
            // If health is 0 or less, ensure player transitions to a death state
            // TODO: context.SwitchState(context.deathState); // Once DeathState is implemented
            Debug.Log("Player is dead. Not transitioning to movement/idle.");
            return;
        }

        // Otherwise, transition back to movement or idle based on current input
        if (context.inputHandler.GetMoveInput().magnitude > 0.1f)
        {
            context.SwitchState(context.movementState);
            Debug.Log("Exiting Hit State: Transitioning to Movement (input detected)");
        }
        else
        {
            context.SwitchState(context.idleState);
            Debug.Log("Exiting Hit State: Transitioning to Idle (no input detected)");
        }
    }
}
