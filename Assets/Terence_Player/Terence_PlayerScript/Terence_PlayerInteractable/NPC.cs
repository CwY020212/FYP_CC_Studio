using UnityEngine;
using TMPro; 

public class NPC : MonoBehaviour, IInteractable
{
    public DialogueInteractionDefinition dialogueInteraction;

    public GameObject CurrentWorldSpacePrompt { get; set; }

    private PlayerStateMachine _player; // Keep a reference to the interacting player

    private void Start()
    {
        // Ensure dialogueInteraction is assigned
        if (dialogueInteraction == null)
        {
            Debug.LogError("NPC: DialogueInteractionDefinition not assigned for " + gameObject.name);
        }
    }

    public bool CanInteract(PlayerStateMachine player)
    {
        // For dialogue, perhaps always true if within range, or false if already in dialogue
        // For simplicity, let's assume always true if within interaction range.
        // You might add logic here to check if a dialogue is already active globally.
        return true;
    }

    public void Interact(PlayerStateMachine player)
    {
        Debug.Log($"Player is talking to {gameObject.name}.");
        _player = player; // Store reference to the player

        // Hide the world space prompt once interaction begins
        if (CurrentWorldSpacePrompt != null)
        {
            InteractionPromptManager.Instance.HidePrompt(this);
        }

        // Lock player movement/input during dialogue
        player.inputHandler.OnDisable(); // Assuming you have a method to disable player input

        // Trigger player animation for talking (if applicable)
        if (!string.IsNullOrEmpty(dialogueInteraction.playerAnimationTrigger))
        {
            player.animator.SetTrigger(dialogueInteraction.playerAnimationTrigger);
        }

        // Open the dedicated dialogue UI
        if (dialogueInteraction.dedicatedUIPrefab != null)
        {
            // Instantiate or activate your dialogue UI
            // This is where you would pass the dialogue lines to your Dialogue UI system
            DialogueUIManager.Instance.StartDialogue(dialogueInteraction.dialogueLines, dialogueInteraction.speakerNames, OnDialogueEnd);
        }
        else
        {
            Debug.LogWarning("No dedicated UI prefab assigned for this dialogue interaction.");
            // If no dedicated UI, maybe just log the first line and immediately end interaction
            if (dialogueInteraction.dialogueLines != null && dialogueInteraction.dialogueLines.Count > 0)
            {
                Debug.Log($"NPC ({gameObject.name}): {dialogueInteraction.dialogueLines[0]}");
            }
            OnDialogueEnd(); // Immediately end if no UI
        }
    }

    public string GetInteractionPrompt()
    {
        return dialogueInteraction.interactionPromptText;
    }

    // This method will be called by your DialogueUIManager when the dialogue is finished
    private void OnDialogueEnd()
    {
        Debug.Log("Dialogue with NPC ended. Re-enabling player input.");
        _player.inputHandler.OnEnable(); // Re-enable player input
        // Potentially switch player state back to idle or movement
        _player.SwitchState(_player.idleState); // Or movementState if they were moving
    }
}
