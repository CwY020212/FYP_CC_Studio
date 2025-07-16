using UnityEngine;

public class EnemyRangeDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private bool showDebugVisuals = true;

    public GameObject DetectedTarget
    {
        get;
        set;
    }

    public GameObject UpdateDetector()
    {
        // Physics.OverlapSphere checks for colliders within a sphere.
        // It's good for quickly checking if anything is within range.
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);

        if (colliders.Length > 0)
        {
            // If any colliders are found, set the first one as the detected target.
            DetectedTarget = colliders[0].gameObject;
        }
        else
        {
            // If no colliders are found, clear the detected target.
            DetectedTarget = null;
        }
        return DetectedTarget;
    }

    private void OnDrawGizmos()
    {
        // Only draw gizmos if 'showDebugVisuals' is true.
        if (showDebugVisuals)
        {
            // Set the color for the gizmo.
            // When a target is detected, it turns red; otherwise, it's green.
            Gizmos.color = DetectedTarget != null ? Color.red : Color.green;

            // Draw a wire sphere representing the detection radius.
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // If a target is detected, draw a line to it for better visualization.
            if (DetectedTarget != null)
            {
                Gizmos.DrawLine(transform.position, DetectedTarget.transform.position);
            }
        }
    }
}
