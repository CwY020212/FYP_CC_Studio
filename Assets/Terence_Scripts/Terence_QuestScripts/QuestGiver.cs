using UnityEngine;

public class QuestGiver : MonoBehaviour, IInteractable
{
    public QuestData questToGive;
    public DialogueInteractionDefinition questOngoingDialogue;
    public DialogueInteractionDefinition introDialogue;
    public DialogueInteractionDefinition questAcceptedDialogue;
    public DialogueInteractionDefinition questAlreadyActiveDialogue;
    public DialogueInteractionDefinition questCompletedDialogue;
    public DialogueInteractionDefinition questStageCompleteDialogue; // Dialogue for when a stage is done and they return

    public string InteractionPromptText { get; private set; }
    public GameObject CurrentWorldSpacePrompt { get; set; }

    private DialogueInteractionDefinition currentInteractionDefinition;

    private bool shouldPlayAcceptedDialogueAfterChoice = false;

    private void Awake()
    {
        UpdateInteractionDefinition();
    }

    private void OnEnable()
    {
        QuestManager.onQuestStarted += OnQuestStatusChanged;
        QuestManager.onQuestCompleted += OnQuestStatusChanged;
        QuestManager.onQuestStageCompleted += OnQuestStatusChanged;
        DialogueManager.onDialogueEnded += OnDialogueEndedCallback;
        DialogueManager.onDialogueChoiceMade += OnDialogueChoiceMadeCallback;
    }

    private void OnDisable()
    {
        QuestManager.onQuestStarted -= OnQuestStatusChanged;
        QuestManager.onQuestCompleted -= OnQuestStatusChanged;
        QuestManager.onQuestStageCompleted -= OnQuestStatusChanged;
        DialogueManager.onDialogueEnded -= OnDialogueEndedCallback;
        DialogueManager.onDialogueChoiceMade -= OnDialogueChoiceMadeCallback;
    }

    private void OnQuestStatusChanged(QuestData changedQuest)
    {
        if (changedQuest == questToGive)
        {
            // Only update if dialogue is NOT active, otherwise it will be updated after dialogue ends.
            // This prevents a race condition or incorrect prompt while dialogue is playing.
            if (DialogueManager.Instance == null || !DialogueManager.Instance.IsDialogueActive())
            {
                UpdateInteractionDefinition();
            }
        }
    }

    private void OnDialogueEndedCallback()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager.Instance is null. Cannot notify UI to show quest.");
            return;
        }

        if (shouldPlayAcceptedDialogueAfterChoice)
        {
            shouldPlayAcceptedDialogueAfterChoice = false;
            Debug.Log("Quest Accepted Dialogue finished. Now notifying to show Quest Task UI.");
            QuestManager.Instance.NotifyShowQuestUI(questToGive);
        }

        // Always update interaction definition after dialogue ends, regardless of why it started
        UpdateInteractionDefinition();
    }

    private void OnDialogueChoiceMadeCallback(int choiceIndex)
    {
        if (currentInteractionDefinition == introDialogue && questToGive != null && questToGive.currentState == QuestData.QuestState.NotStarted)
        {
            if (choiceIndex == 0) // Accept Quest
            {
                QuestManager.Instance.AssignQuest(questToGive);
                Debug.Log($"Quest '{questToGive.questName}' accepted!");

                shouldPlayAcceptedDialogueAfterChoice = true;
                currentInteractionDefinition = questAcceptedDialogue;

                if (questAcceptedDialogue != null)
                {
                    DialogueManager.Instance.StartDialogue(
                        questAcceptedDialogue.dialogueLines,
                        questAcceptedDialogue.speakerNames,
                        questAcceptedDialogue
                    );
                    QuestManager.Instance.NotifyShowQuestUI(questToGive); // Notify UI immediately on quest start
                }
                else
                {
                    Debug.LogWarning($"QuestGiver on {gameObject.name} has no questAcceptedDialogue assigned. Showing Quest UI immediately.");
                    shouldPlayAcceptedDialogueAfterChoice = false;
                    DialogueManager.Instance.EndDialogue();
                }
            }
            else // Decline Quest
            {
                Debug.Log($"Quest '{questToGive.questName}' declined.");
                DialogueManager.Instance.EndDialogue();
                UpdateInteractionDefinition(); // Update state after declining
            }
        }
    }

    private void UpdateInteractionDefinition()
    {
        DialogueInteractionDefinition definitionToUse = introDialogue;

        if (questToGive != null)
        {
            switch (questToGive.currentState)
            {
                case QuestData.QuestState.NotStarted:
                    definitionToUse = introDialogue;
                    break;
                case QuestData.QuestState.Active:
                    QuestData.QuestStage currentStage = questToGive.GetCurrentStage();
                    if (currentStage != null)
                    {
                        if (currentStage.requiresReturnToGiver && currentStage.objectiveTargetID == this.name)
                        {
                            if (questToGive.IsCurrentStageComplete())
                            {
                                // Player is back, and requirements for this return stage are met
                                definitionToUse = questStageCompleteDialogue; // Use dialogue for stage completion
                            }
                            else
                            {
                                // Player is back, but hasn't met the "return to giver" requirement (e.g. didn't have the item needed yet if it was a delivery stage ending in a return)
                                // This scenario is less common for simple "talk to complete" return stages,
                                // but if it occurs, maybe it's still just the ongoing dialogue.
                                definitionToUse = questOngoingDialogue;
                            }
                        }
                        else // Not a 'return to giver' stage, or not this specific giver
                        {
                            // Quest is active, but player is still out there doing objectives (e.g., collecting items)
                            definitionToUse = questOngoingDialogue;
                        }
                    }
                    else // Should ideally not happen if quest is Active and has stages
                    {
                        definitionToUse = questOngoingDialogue; // Fallback
                    }
                    break;
                case QuestData.QuestState.Completed:
                    definitionToUse = questCompletedDialogue;
                    break;
                case QuestData.QuestState.Failed:
                    definitionToUse = questAlreadyActiveDialogue; // Or a specific failed quest dialogue
                    break;
            }
        }


        if (definitionToUse == null)
        {
            definitionToUse = introDialogue; // Fallback to intro if nothing else
            if (definitionToUse == null)
            {
                Debug.LogWarning($"QuestGiver on {gameObject.name} has no valid dialogue definition assigned for current state. Interaction prompt set to 'Interact'.");
                InteractionPromptText = "Interact";
                return;
            }
        }

        // Only update currentInteractionDefinition and InteractionPromptText if dialogue isn't active
        // This prevents the prompt from changing mid-dialogue.
        if (DialogueManager.Instance == null || !DialogueManager.Instance.IsDialogueActive())
        {
            currentInteractionDefinition = definitionToUse;
            InteractionPromptText = currentInteractionDefinition.interactionPromptText;
        }
        else
        {
            Debug.Log($"QuestGiver {gameObject.name} deferred UpdateInteractionDefinition because DialogueManager is active.");
        }
    }

    public bool CanInteract(PlayerStateMachine player)
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            return false;
        }
        return true;
    }

    public void Interact(PlayerStateMachine player)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager not found in scene!");
            return;
        }

        if (questToGive != null && questToGive.currentState == QuestData.QuestState.Active)
        {
            QuestData.QuestStage currentStage = questToGive.GetCurrentStage();
            if (currentStage != null && currentStage.requiresReturnToGiver && currentStage.objectiveTargetID == this.name)
            {
                
                if (!questToGive.IsCurrentStageComplete()) // Only advance if NOT already complete
                {
                    Debug.Log($"Player is interacting with QuestGiver to complete 'return to giver' stage for quest '{questToGive.questName}'.");
                    QuestManager.Instance.UpdateQuestProgress(questToGive.questName, this.name, 1);
                }
                // Update the interaction definition *after* attempting to progress,
                // as the quest state might have changed (e.g., stage completed, or full quest completed).
                UpdateInteractionDefinition();
                // Now, determine the dialogue based on the *new* state/stage
                if (questToGive.IsComplete())
                {
                    currentInteractionDefinition = questCompletedDialogue;
                }
                else // Quest advanced to next stage, but not fully complete
                {
                    currentInteractionDefinition = questStageCompleteDialogue; // This will now be the dialogue for the *next* stage or final return if stage 1 was the last.
                }
            }
            // If it's an active quest but NOT a return-to-giver stage for THIS giver,
            // we still want to play the ongoing dialogue.
            else if (currentInteractionDefinition == null || currentInteractionDefinition == introDialogue || currentInteractionDefinition == questAcceptedDialogue)
            {
                // This ensures if it's an ongoing quest (not a return-to-giver for THIS NPC)
                // or if it was just accepted, it still updates to ongoing dialogue.
                UpdateInteractionDefinition();
            }
        }
        // If it's a new quest, or accepted dialogue is supposed to play after choice
        else if (currentInteractionDefinition == null || currentInteractionDefinition == introDialogue || currentInteractionDefinition == questAcceptedDialogue)
        {
            UpdateInteractionDefinition();
        }


        if (currentInteractionDefinition != null)
        {
            DialogueManager.Instance.StartDialogue(
                currentInteractionDefinition.dialogueLines,
                currentInteractionDefinition.speakerNames,
                currentInteractionDefinition
            );
        }
        else
        {
            Debug.LogWarning($"QuestGiver on {gameObject.name} has no dialogue to play for its current state after attempting to resolve it.");
        }
    }

    public string GetInteractionPrompt()
    {
        return InteractionPromptText;
    }
}
