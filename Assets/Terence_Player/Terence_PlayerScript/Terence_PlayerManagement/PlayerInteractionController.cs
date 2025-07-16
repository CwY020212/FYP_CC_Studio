using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private PlayerStateMachine playerStateMachine;
    private IInteractable currentClosestInteractable; // The one currently in range and valid

    public Transform playerPivot; // Assign a transform (e.g., player's head or chest) for raycasting

    private void Awake()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
        // Ensure PlayerInteractionController is added to RequireComponent in PlayerStateMachine
        // or that it's manually added to the same GameObject as PlayerStateMachine.
    }

    private void Update()
    {
        // Perform a raycast to detect interactables in front of the player
        if (Physics.Raycast(playerPivot.position, playerPivot.forward, out RaycastHit hit, interactionRange, interactableLayer))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                // Check if the detected interactable is currently interactable (e.g., not busy, quest conditions met)
                if (interactable.CanInteract(playerStateMachine))
                {
                    // If a new valid interactable is found, hide the old prompt and show the new one
                    if (currentClosestInteractable != interactable)
                    {
                        InteractionPromptManager.Instance?.HidePrompt(currentClosestInteractable); // Hide old prompt
                        currentClosestInteractable = interactable; // Update to the new interactable

                        // Show prompt for the new interactable, using its GameObject's position
                        Vector3 promptTargetWorldPosition = ((MonoBehaviour)interactable).transform.position;
                        currentClosestInteractable.CurrentWorldSpacePrompt = InteractionPromptManager.Instance?.ShowPrompt(interactable, promptTargetWorldPosition);
                    }
                }
                else
                {
                    // Interactable is in range but not currently interactable (e.g., dialogue active, player lacks item)
                    if (currentClosestInteractable == interactable) // If it was the one we were showing a prompt for
                    {
                        InteractionPromptManager.Instance?.HidePrompt(interactable);
                        currentClosestInteractable = null;
                    }
                }
            }
            else // Raycast hit something on the interactable layer, but it's not an IInteractable
            {
                InteractionPromptManager.Instance?.HidePrompt(currentClosestInteractable);
                currentClosestInteractable = null;
            }
        }
        else
        {
            // No interactable found within range
            InteractionPromptManager.Instance?.HidePrompt(currentClosestInteractable);
            currentClosestInteractable = null;
        }

        // Assign the detected interactable to the state machine for the InteractState to use
        playerStateMachine.currentTargetInteractable = currentClosestInteractable;

        // Check for interact input from the player
        if (playerStateMachine.inputHandler.GetInteractInputDown() && playerStateMachine.currentTargetInteractable != null)
        {
            // Only allow switching to the interact state if the player is currently idle or moving
            // This prevents interrupting other critical states (e.g., attacking, dodging)
            if (playerStateMachine.currentState == playerStateMachine.idleState ||
                playerStateMachine.currentState == playerStateMachine.movementState ||
                playerStateMachine.currentState == playerStateMachine.runState)
            {
                playerStateMachine.SwitchState(playerStateMachine.interactState);
            }
        }
    }
}
