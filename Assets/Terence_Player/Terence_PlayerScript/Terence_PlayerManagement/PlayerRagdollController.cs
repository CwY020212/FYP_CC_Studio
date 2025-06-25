using UnityEngine;

public class PlayerRagdollController : MonoBehaviour
{
    [Tooltip("All Rigidbodies on the ragdoll body parts. Assign these manually or populate via script.")]
    public Rigidbody[] ragdollRigidbodies;
    [Tooltip("All Colliders on the ragdoll body parts. These will be enabled/disabled as physical colliders.")]
    public Collider[] ragdollPhysicalColliders; // These should NOT be triggers when ragdolling

    [Header("References")]
    [Tooltip("The main Animator component on the player.")]
    public Animator playerAnimator;
    [Tooltip("The main CharacterController component on the player.")]
    public CharacterController characterController;

    private bool isRagdolling = false;
    public bool IsRagdolling => isRagdolling; // Public getter

    void Awake()
    {
        // Auto-populate if not assigned manually (only finds direct children, adjust if ragdoll is deeper)
        if (ragdollRigidbodies == null || ragdollRigidbodies.Length == 0)
        {
            ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        }
        if (ragdollPhysicalColliders == null || ragdollPhysicalColliders.Length == 0)
        {
            ragdollPhysicalColliders = GetComponentsInChildren<Collider>();
        }

        DisableRagdoll(); // Start with ragdoll physics disabled
    }

    /// <summary>
    /// Disables ragdoll physics, re-enables animator and CharacterController.
    /// </summary>
    public void DisableRagdoll()
    {
        isRagdolling = false;

        if (playerAnimator != null) playerAnimator.enabled = true;
        if (characterController != null) characterController.enabled = true;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = true; // Make rigidbodies kinematic (not affected by physics)
        }
        foreach (Collider col in ragdollPhysicalColliders)
        {
            // For the *physical* ragdoll colliders, they should be enabled and NOT triggers.
            // If some of your "Ragdoll" layer colliders are *only* for hit detection (triggers),
            // ensure they are separate from the ones assigned to ragdollPhysicalColliders.
            col.enabled = false; // Disable physical colliders when not ragdolling
            // col.isTrigger = false; // Ensure they are not triggers for physics simulation
        }
    }

    /// <summary>
    /// Enables ragdoll physics, disables animator and CharacterController, and applies an initial impulse.
    /// </summary>
    /// <param name="impulseDirection">Direction of the initial force.</param>
    /// <param name="forceMagnitude">Magnitude of the initial force.</param>
    public void EnableRagdoll(Vector3 impulseDirection, float forceMagnitude)
    {
        isRagdolling = true;

        if (playerAnimator != null) playerAnimator.enabled = false;
        if (characterController != null) characterController.enabled = false;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = false; // Allow physics to affect it
            rb.linearVelocity = Vector3.zero; // Clear any previous velocity
            rb.AddForce(impulseDirection * forceMagnitude, ForceMode.Impulse); // Apply the initial knockback impulse
        }
        foreach (Collider col in ragdollPhysicalColliders)
        {
            col.enabled = true; // Enable physical colliders
            // col.isTrigger = false; // Ensure they are not triggers for physics simulation
        }
    }
}
