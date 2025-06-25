using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private PlayerStateMachine playerStateMachine;
    private IInteractable currentClosestInteractable; // The one currently in range and valid

    public Transform playerPivot;
    private void Awake()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
        // Make sure PlayerInteractionController is added to RequireComponent in PlayerStateMachine.
    }

    private void Update()
    {
        // Example: Raycast forward to find interactables
        if (Physics.Raycast(playerPivot.position, transform.forward, out RaycastHit hit, interactionRange, interactableLayer))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (interactable.CanInteract(playerStateMachine))
                {
                    // If a new interactable, show prompt
                    if (currentClosestInteractable != interactable)
                    {
                        InteractionPromptManager.Instance?.HidePrompt(currentClosestInteractable); // Hide old
                        // Pass the interactable's transform.position for a fixed prompt location
                        // You can also add a specific "prompt anchor" Transform to IInteractable
                        currentClosestInteractable = interactable; // Update before showing new prompt

                        // --- IMPORTANT CHANGE HERE ---
                        // Use the interactable's GameObject position for the prompt
                        Vector3 promptTargetWorldPosition = ((MonoBehaviour)interactable).transform.position;
                        currentClosestInteractable.CurrentWorldSpacePrompt = InteractionPromptManager.Instance?.ShowPrompt(interactable, promptTargetWorldPosition);
                    }
                }
                else
                {
                    // Interactable is not valid right now (e.g., player lacks item, not enough currency)
                    InteractionPromptManager.Instance?.HidePrompt(interactable);
                    if (currentClosestInteractable == interactable) currentClosestInteractable = null;
                }
            }
        }
        else
        {
            // No interactable found
            InteractionPromptManager.Instance?.HidePrompt(currentClosestInteractable);
            currentClosestInteractable = null;
        }

        // Assign the detected interactable to the state machine
        // The PlayerInteractState will read this property.
        playerStateMachine.currentTargetInteractable = currentClosestInteractable;

        // Check for interact input
        if (playerStateMachine.inputHandler.GetInteractInputDown() && playerStateMachine.currentTargetInteractable != null)
        {
            // Only switch to interact state if player is not already busy
            if (playerStateMachine.currentState == playerStateMachine.idleState ||
                playerStateMachine.currentState == playerStateMachine.movementState ||
                playerStateMachine.currentState == playerStateMachine.runState)
            {
                playerStateMachine.SwitchState(playerStateMachine.interactState);
            }
        }
    }
}
