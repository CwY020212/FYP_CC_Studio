using UnityEngine;

public class EnemyHitTester : MonoBehaviour
{
    [Header("Testing Settings")]
    [Tooltip("Reference to the PlayerStateMachine. Assign this in the Inspector.")]
    public PlayerStateMachine playerStateMachine;

    [Tooltip("The range within which the player must be for the 'H' key to trigger a hit.")]
    public float testAttackRange = 3.0f; // Adjust this range as needed

    [Header("Simulated Hit Data")]
    [Tooltip("Damage dealt by this test attack.")]
    public int simulatedDamageOutput = 15;
    [Tooltip("Force of the knockback.")]
    public float simulatedKnockbackStrength = 7f;
    [Tooltip("How long the player is stunned.")]
    public float simulatedStunDuration = 0.8f;
    [Tooltip("Should this test attack trigger the full ragdoll on hit?")]
    public bool simulatedCausesFullRagdoll = false;
    [Tooltip("Initial impulse for ragdoll (direction relative to *this* enemy's forward).")]
    public Vector3 simulatedRagdollImpulseDirection = Vector3.forward; // e.g., forward relative to enemy

    void Update()
    {
        // Check if the 'H' key is pressed down
        if (Input.GetKeyDown(KeyCode.H))
        {
            // Ensure we have a reference to the PlayerStateMachine
            if (playerStateMachine == null)
            {
                // Try to find it if not assigned (less efficient, but useful for quick tests)
                playerStateMachine = FindObjectOfType<PlayerStateMachine>();
                if (playerStateMachine == null)
                {
                    Debug.LogError("PlayerStateMachine not found in scene or assigned. Cannot test hit.", this);
                    return;
                }
            }

            // Check if the player is within the defined test attack range
            float distanceToPlayer = Vector3.Distance(transform.position, playerStateMachine.transform.position);

            if (distanceToPlayer <= testAttackRange)
            {
                Debug.Log($"[PlayerHitTester] Player is within range ({distanceToPlayer:F2}m). Triggering simulated hit!");
                SimulatePlayerHit();
            }
            else
            {
                Debug.Log($"[PlayerHitTester] Player is too far ({distanceToPlayer:F2}m). Test attack range is {testAttackRange}m. Get closer and press 'H' again.");
            }
        }
    }

    private void SimulatePlayerHit()
    {
        // Create the HitData struct with the simulated values
        HitData simulatedHitData = new HitData
        {
            damageAmount = simulatedDamageOutput,
            // For testing, we can use the player's current position as the hit point
            hitPoint = playerStateMachine.transform.position,
            // The position of this test enemy is the attacker's position
            attackerPosition = transform.position,
            knockbackForce = simulatedKnockbackStrength,
            hitStunDuration = simulatedStunDuration,
            activatesFullRagdoll = simulatedCausesFullRagdoll,
            // Convert the local ragdoll impulse direction (relative to this enemy) to world space
            initialRagdollImpulse = transform.TransformDirection(simulatedRagdollImpulseDirection).normalized
        };

        // Call the PlayerStateMachine's TakeHit method
        playerStateMachine.TakeHit(simulatedHitData);
    }

    // Optional: Draw a debug sphere in the editor to visualize the test attack range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; // Choose a distinct color
        Gizmos.DrawWireSphere(transform.position, testAttackRange);
    }
}
