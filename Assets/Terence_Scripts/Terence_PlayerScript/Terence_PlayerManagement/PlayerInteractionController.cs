using TMPro;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private PlayerStateMachine playerStateMachine;
    private IInteractable currentClosestInteractable; // The one currently in range and valid

    [Header("OverlapBox Settings")]
    [SerializeField] private Vector3 interactionBoxHalfExtents = new Vector3(0.5f, 0.5f, 1f); // Half size of the box (x,y,z)
    [SerializeField] private Vector3 interactionBoxOffset = new Vector3(0, 0, 1f); // Offset from playerPivot.forward

    public Transform playerPivot; // Assign a transform (e.g., player's head or chest) for raycasting

    private void Awake()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    private void Update()
    {
        IInteractable previouslyClosestInteractable = currentClosestInteractable;
        currentClosestInteractable = null; // Reset for this frame's detection

        // Perform a raycast to detect interactables in front of the player
        if (Physics.Raycast(playerPivot.position, playerPivot.forward, out RaycastHit hit, interactionRange, interactableLayer))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (interactable.CanInteract(playerStateMachine))
                {
                    currentClosestInteractable = interactable; // Valid interactable found
                }
            }
        }

        // --- UI Prompt Logic using InteractionPromptManager ---
        if (currentClosestInteractable != null)
        {
            // If a new valid interactable is found or the prompt needs updating
            if (previouslyClosestInteractable != currentClosestInteractable ||
                (InteractionPromptManager.Instance != null && !InteractionPromptManager.Instance.IsPromptVisible())) // Check if prompt isn't visible
            {
                // Hide any existing prompt first if it was for a different interactable
                if (previouslyClosestInteractable != null && previouslyClosestInteractable != currentClosestInteractable)
                {
                    InteractionPromptManager.Instance?.HidePrompt(previouslyClosestInteractable);
                }

                // Show prompt for the new interactable
                Vector3 promptTargetWorldPosition = ((MonoBehaviour)currentClosestInteractable).transform.position; // Get position from the MonoBehavior part of the IInteractable
                currentClosestInteractable.CurrentWorldSpacePrompt = InteractionPromptManager.Instance?.ShowPrompt(currentClosestInteractable, promptTargetWorldPosition);
            }
            // If it's the same interactable and prompt is already visible, no action needed for ShowPrompt,
            // as InteractionPromptManager handles the text update if it's the same interactable.
            // However, QuestGiver's UpdateInteractionDefinition might have changed the prompt text
            // without the IInteractable itself changing. We need to ensure the text is always fresh.
            if (InteractionPromptManager.Instance != null && currentClosestInteractable.CurrentWorldSpacePrompt != null)
            {
                InteractionPromptManager.Instance.UpdatePromptText(currentClosestInteractable, currentClosestInteractable.GetInteractionPrompt());
            }

        }
        else
        {
            // No interactable found or current one is no longer interactable
            if (previouslyClosestInteractable != null)
            {
                InteractionPromptManager.Instance?.HidePrompt(previouslyClosestInteractable);
                // Also clear the CurrentWorldSpacePrompt reference
                if (previouslyClosestInteractable.CurrentWorldSpacePrompt != null)
                {
                    previouslyClosestInteractable.CurrentWorldSpacePrompt = null;
                }
            }
        }


        // Assign the detected interactable to the state machine for the InteractState to use
        playerStateMachine.currentTargetInteractable = currentClosestInteractable;

        // Check for interact input from the player
        if (playerStateMachine.inputHandler.GetInteractInputDown() && playerStateMachine.currentTargetInteractable != null)
        {
            // Only allow switching to the interact state if the player is currently idle or moving
            if (playerStateMachine.currentState == playerStateMachine.idleState ||
                playerStateMachine.currentState == playerStateMachine.movementState ||
                playerStateMachine.currentState == playerStateMachine.runState)
            {
                playerStateMachine.SwitchState(playerStateMachine.interactState);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Ensure playerPivot is assigned before drawing
        if (playerPivot == null)
        {
            Debug.LogWarning("Player Pivot not assigned for PlayerInteractionController Gizmos.", this);
            return;
        }

        // Calculate the center and orientation for the Gizmo
        Vector3 gizmoCenter = playerPivot.position + playerPivot.rotation * interactionBoxOffset;
        Quaternion gizmoOrientation = playerPivot.rotation;

        // Draw the OverlapBox
        Gizmos.matrix = Matrix4x4.TRS(gizmoCenter, gizmoOrientation, Vector3.one); // Apply position, rotation, scale
        Gizmos.color = Color.magenta; // Choose a distinct color for the box
        Gizmos.DrawWireCube(Vector3.zero, interactionBoxHalfExtents * 2); // Draw a cube at origin, scaled by halfExtents * 2
        Gizmos.matrix = Matrix4x4.identity; // Reset Gizmo matrix to default after drawing custom shape

        // Optional: Draw a sphere for the currently detected interactable if one exists
        if (currentClosestInteractable != null)
        {
            Gizmos.color = Color.green;
            if (currentClosestInteractable is MonoBehaviour monoBehaviour)
            {
                Gizmos.DrawSphere(monoBehaviour.transform.position, 0.2f);
            }
        }
    }
}
