using UnityEngine;

public class PlayerInteractState : PlayerBaseState
{
    private bool interactionAnimationFinished = false;

    public PlayerInteractState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        Debug.Log("Entering Interact State.");
        interactionAnimationFinished = false;

        // Determine which animation to play based on the interactable type
        if (context.currentTargetInteractable is NPC npc && npc.dialogueInteraction != null)
        {
            if (!string.IsNullOrEmpty(npc.dialogueInteraction.playerAnimationTrigger))
            {
                context.animator.SetTrigger(npc.dialogueInteraction.playerAnimationTrigger);
                Debug.Log($"Triggering player animation: {npc.dialogueInteraction.playerAnimationTrigger}");
            }
            else
            {
                Debug.Log("No specific player animation trigger for NPC interaction. Animation considered finished immediately.");
                HandleInteractionAnimationEnd();
            }
        }

        else if (context.currentTargetInteractable is QuestTrigger) // Check if it's a QuestTrigger
        {
            context.animator.SetTrigger("PickUp"); // Trigger the "PickUp" animation
            Debug.Log("Triggering 'PickUp' animation for QuestTrigger interaction.");
        }
        else
        {
            // Fallback for other interactables or if no specific trigger is found
            context.animator.SetTrigger("Talk"); // Or a generic "Interact_Default"
            Debug.Log("Triggering generic 'Talk' animation.");
            // Ensure this animation also has an event or a mechanism to call HandleInteractionAnimationEnd.
        }

        // Subscribe to the animation end event from the PlayerStateMachine
        context.OnInteractionAnimationEnd += HandleInteractionAnimationEnd;

        // Perform the interaction (e.g., collect the wood, start dialogue)
        // This happens concurrently with the animation starting.
        PerformInteraction();
    }

    public override void UpdateState()
    {
        // The player should remain in the InteractState as long as:
        // 1. The interaction animation is still playing, OR
        // 2. Dialogue is currently active.
        // Only when BOTH are finished should we allow checking for state changes (e.g., back to idle/movement).

        // --- FIX HERE: Un-comment and use IsDialogueActive() ---
        bool dialogueIsActive = DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();

        if (interactionAnimationFinished && !dialogueIsActive) // Both conditions must be met to transition out
        {
            Debug.Log("Animation finished AND dialogue ended. Checking for state change.");
            CheckForStateChange();
        }
        else
        {
            // Debug.Log($"Staying in Interact State. Animation finished: {interactionAnimationFinished}, Dialogue Active: {dialogueIsActive}");
            // Stay in Interact State if animation is still playing OR dialogue is active
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Interact State.");
        // Reset any triggers that might have been set
        context.animator.ResetTrigger("Talk");
        context.animator.ResetTrigger("Interact_Default"); // Reset a common generic trigger if used

        if (context.currentTargetInteractable is NPC npc && npc.dialogueInteraction != null)
        {
            if (!string.IsNullOrEmpty(npc.dialogueInteraction.playerAnimationTrigger))
            {
                context.animator.ResetTrigger(npc.dialogueInteraction.playerAnimationTrigger);
            }
        }
        context.currentTargetInteractable = null;
        // Unsubscribe to prevent memory leaks and unwanted calls
        context.OnInteractionAnimationEnd -= HandleInteractionAnimationEnd;
    }

    private void PerformInteraction()
    {
        if (context.currentTargetInteractable != null)
        {
            context.currentTargetInteractable.Interact(context);
            Debug.Log($"Player initiated interaction with: {context.currentTargetInteractable.GetType().Name}");
        }
        else
        {
            Debug.LogWarning("PlayerInteractState entered but no currentTargetInteractable set! Transitioning to Idle.");
            // Immediately transition if no target to interact with
            context.SwitchState(context.idleState);
        }
    }

    private void HandleInteractionAnimationEnd()
    {
        Debug.Log("Interaction animation finished callback received.");
        interactionAnimationFinished = true;
    }

    private void CheckForStateChange()
    {
        // This method is only called when interactionAnimationFinished is true
        // AND DialogueManager.Instance.IsDialogueActive() is false (checked in UpdateState).
        // So, it's safe to check for player movement input here to decide next state.
        if (context.inputHandler.GetMoveInput().magnitude > 0.1f)
        {
            context.SwitchState(context.movementState);
        }
        else
        {
            context.SwitchState(context.idleState);
        }
    }
}