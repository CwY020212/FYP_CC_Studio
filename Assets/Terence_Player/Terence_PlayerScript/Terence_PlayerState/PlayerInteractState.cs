using UnityEngine;

public class PlayerInteractState : PlayerBaseState
{
    private bool interactionAnimationFinished = false;

    public PlayerInteractState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        Debug.Log("Entering Interact State.");
        interactionAnimationFinished = false;

        // Trigger interaction animation (e.g., player reaching out)
        // This might be more generic for talking, or specific based on the InteractionDefinition
        if (context.currentTargetInteractable is NPC npc && npc.dialogueInteraction != null)
        {
            if (!string.IsNullOrEmpty(npc.dialogueInteraction.playerAnimationTrigger))
            {
                context.animator.SetTrigger(npc.dialogueInteraction.playerAnimationTrigger);
            }
            else
            {
                // No specific animation, consider it finished immediately
                HandleInteractionAnimationEnd();
            }
        }
        else
        {
            context.animator.SetTrigger("Interact_Default"); // Or a generic interaction animation
        }

        context.OnInteractionAnimationEnd += HandleInteractionAnimationEnd;

        // Perform the interaction. For an NPC, this will kick off the dialogue.
        PerformInteraction();
    }

    public override void UpdateState()
    {
        // NO INPUT CHECKS HERE during the animation lock
        // The player is effectively "locked" until interactionAnimationFinished is true.
        // For dialogue, the player might stay in this state until dialogue ends
        // (which is handled by the OnDialogueEnd callback from DialogueUIManager).

        // If your interaction animation is very short and the dialogue system handles the "lock",
        // you might transition to an "Interacting" or "Dialogue" state after animation.
        // For now, if the animation is done, and dialogue has finished, allow transition.
        if (interactionAnimationFinished )//&& !DialogueUIManager.Instance.IsDialogueActive()) // Assuming DialogueUIManager has an IsDialogueActive()
        {
            CheckForStateChange();
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Interact State.");
        context.animator.ResetTrigger("Interact_Default"); // Reset relevant triggers
        if (context.currentTargetInteractable is NPC npc && npc.dialogueInteraction != null)
        {
            if (!string.IsNullOrEmpty(npc.dialogueInteraction.playerAnimationTrigger))
            {
                context.animator.ResetTrigger(npc.dialogueInteraction.playerAnimationTrigger);
            }
        }
        context.currentTargetInteractable = null;
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
            Debug.LogWarning("PlayerInteractState entered but no currentTargetInteractable set!");
            // Immediately transition if no target
            context.SwitchState(context.idleState);
        }
    }

    private void HandleInteractionAnimationEnd()
    {
        Debug.Log("Interaction animation finished, allowing state change.");
        interactionAnimationFinished = true;
    }

    private void CheckForStateChange()
    {
        // Only allow state change AFTER the interaction animation has finished
        // AND after the dialogue (if any) has ended.
        if (interactionAnimationFinished) // and !DialogueUIManager.Instance.IsDialogueActive() if you want the state to be held by dialogue
        {
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
}