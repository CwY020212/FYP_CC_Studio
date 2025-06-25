using UnityEngine;

public class EnemyAttackCollider : MonoBehaviour
{
    [Tooltip("Damage dealt by this attack.")]
    public int damageOutput = 15;
    [Tooltip("Force of the knockback.")]
    public float knockbackStrength = 7f;
    [Tooltip("How long the player is stunned.")]
    public float stunDuration = 0.8f;
    [Tooltip("Should this attack trigger the full ragdoll on hit?")]
    public bool causesFullRagdoll = false;
    [Tooltip("Initial impulse for ragdoll (direction relative to attacker forward).")]
    public Vector3 ragdollImpulseDirection = Vector3.forward; // Example: forward relative to attacker

    private bool hasHitThisAttack = false; // Prevents multiple hits from one swing

    void OnEnable()
    {
        // Reset hit flag when hitbox is enabled (e.g., at start of attack animation)
        hasHitThisAttack = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Only hit once per activation
        if (hasHitThisAttack) return;

        // Check if the collider belongs to the Player's "Ragdoll" layer (your trigger hitboxes)
        // Or if the root parent has the "Player" tag.
        if (other.gameObject.layer == LayerMask.NameToLayer("Ragdoll") || other.CompareTag("Player"))
        {
            // Find the PlayerStateMachine component in the parent hierarchy
            PlayerStateMachine playerStateMachine = other.GetComponentInParent<PlayerStateMachine>();

            if (playerStateMachine != null)
            {
                // Create the HitData struct
                HitData hitData = new HitData
                {
                    damageAmount = damageOutput,
                    hitPoint = other.ClosestPoint(transform.position), // Approximate point of contact
                    attackerPosition = transform.root.position, // Use enemy's root position as attacker origin
                    knockbackForce = knockbackStrength,
                    hitStunDuration = stunDuration,
                    activatesFullRagdoll = causesFullRagdoll,
                    // Convert local impulse direction to world space for ragdoll
                    initialRagdollImpulse = transform.TransformDirection(ragdollImpulseDirection).normalized
                };

                playerStateMachine.TakeHit(hitData); // Call the new TakeHit method

                hasHitThisAttack = true; // Mark that this attack has registered a hit
            }
            else
            {
                Debug.LogWarning($"Enemy attack hit {other.name}, but no PlayerStateMachine found in parent.");
            }
        }
    }

    // Call this method via Animation Event at the start of an attack swing
    public void ResetHitbox()
    {
        hasHitThisAttack = false;
        // Optionally, enable the collider/script if it's disabled during idle
        // this.enabled = true;
    }

    // Call this method via Animation Event at the end of an attack swing
    public void DisableHitbox()
    {
        // this.enabled = false;
    }
}
