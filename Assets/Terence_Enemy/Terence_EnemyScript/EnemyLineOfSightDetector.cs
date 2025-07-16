using UnityEngine;

public class EnemyLineOfSightDetector : MonoBehaviour
{
    [SerializeField] float detectionRange = 10.0f;
    [SerializeField] float detectionHeight = 3.0f;
    [SerializeField] LayerMask playerLayerMask;
    [SerializeField] bool showDebugVisuals = true;

    // A private field to store the last detected target for Gizmos drawing
    private GameObject lastDetectedTarget = null;

    public GameObject PerformDetection(GameObject potentialTarget)
    {
        if (potentialTarget == null)
        {
            lastDetectedTarget = null;
            return null;
        }

        RaycastHit hit;
        // Calculate the direction from the enemy's elevated detection point to the potential target's position
        Vector3 startPoint = transform.position + Vector3.up * detectionHeight;
        Vector3 direction = potentialTarget.transform.position - startPoint;

        // Perform the Raycast:
        // - From 'startPoint' (enemy's position + height offset)
        // - In 'direction' towards the target
        // - Store hit information in 'hit'
        // - Max distance 'detectionRange'
        // - Only hit objects on 'playerLayerMask'
        // - QueryTriggerInteraction.Ignore to not hit trigger colliders unless explicitly wanted
        bool hasHit = Physics.Raycast(startPoint, direction, out hit, detectionRange, playerLayerMask, QueryTriggerInteraction.Ignore);

        // Debug.DrawLine should ideally be in Update or a similar method that runs every frame
        // for it to be visible in play mode, but it's fine here for immediate feedback during detection.
        // We'll move the primary debug drawing to OnDrawGizmos for editor visibility.
        if (showDebugVisuals)
        {
            // Draw a temporary debug line based on whether the target was hit.
            // This is primarily for runtime debugging in the Game view if needed.
            if (hasHit && hit.collider.gameObject == potentialTarget)
            {
                Debug.DrawLine(startPoint, hit.point, Color.green, 0.1f); // Line to hit point, visible for 0.1s
            }
            else
            {
                // Draw a red line to the potential target's position if not detected, 
                // or a line indicating the maximum range if no specific target is in mind.
                // For better visualization in OnDrawGizmos, we'll store the hit result.
                Debug.DrawLine(startPoint, startPoint + direction.normalized * detectionRange, Color.red, 0.1f);
            }
        }

        if (hasHit && hit.collider.gameObject == potentialTarget)
        {
            lastDetectedTarget = hit.collider.gameObject; // Store for Gizmos
            return hit.collider.gameObject;
        }
        else
        {
            lastDetectedTarget = null; // Clear if no target is detected
            return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugVisuals)
        {
            return; // Don't draw if visuals are disabled
        }

        // Set the gizmo color
        Gizmos.color = lastDetectedTarget != null ? Color.green : Color.red;

        // Calculate the start point for the gizmo ray
        Vector3 startPoint = transform.position + Vector3.up * detectionHeight;

        // Draw the main detection ray/line
        if (lastDetectedTarget != null)
        {
            // If a target was successfully detected, draw a line directly to it
            Gizmos.DrawLine(startPoint, lastDetectedTarget.transform.position);
            // Draw a small sphere at the detected target's position
            Gizmos.DrawSphere(lastDetectedTarget.transform.position, 0.2f);
        }
        else
        {
            // If no target (or target lost), draw a forward ray indicating the maximum detection range
            // This assumes the enemy has a forward direction, adjust as needed.
            // For a general line of sight, you might just draw a sphere or a cone.
            // For a single ray, consider drawing it in the transform.forward direction.
            Gizmos.DrawLine(startPoint, startPoint + transform.forward * detectionRange);
        }

        // Draw a small sphere at the detection origin
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(startPoint, 0.5f); // A small sphere at the "eye" level of the enemy
    }
}
