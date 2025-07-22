using UnityEngine;

public class QuestGiver : MonoBehaviour, IInteractable
{
    public QuestData questToGive;
    public DialogueInteractionDefinition introDialogue;
    public DialogueInteractionDefinition questOngoingDialogue;
    public DialogueInteractionDefinition questAcceptedDialogue; // Now purely for post-acceptance dialogue
    public DialogueInteractionDefinition questAlreadyActiveDialogue;
    public DialogueInteractionDefinition questCompletedDialogue;
    public DialogueInteractionDefinition questStageCompleteDialogue; // Dialogue for when a stage is done and they return

    public string InteractionPromptText { get; private set; }
    public GameObject CurrentWorldSpacePrompt { get; set; }

    private DialogueInteractionDefinition currentInteractionDefinition; // This stores the current top-level dialogue the giver would offer.

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
        DialogueManager.onDialogueChoiceMade += OnDialogueChoiceMadeCallback; // Still needed for internal logic
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

        if (questToGive != null && questToGive.currentState == QuestData.QuestState.Active && questToGive.currentProgressInStage == 0)
        {
            Debug.Log("Dialogue ended and quest is now active. Notifying to show Quest Task UI.");
            QuestManager.Instance.NotifyShowQuestUI(questToGive);
        }

        // Always update interaction definition after dialogue ends, regardless of why it started
        UpdateInteractionDefinition();
    }

    private void OnDialogueChoiceMadeCallback(int choiceIndex)
    {
        DialogueInteractionDefinition dialogueThatOfferedChoice = DialogueManager.Instance.GetCurrentActiveDialogueDefinition();

        if (dialogueThatOfferedChoice != null)
        {
            // Ensure that the choice index is valid for the nextDialogueDefinitions list
            if (choiceIndex >= 0 && choiceIndex < dialogueThatOfferedChoice.nextDialogueDefinitions.Count)
            {
                DialogueInteractionDefinition chosenNextDialogue = dialogueThatOfferedChoice.nextDialogueDefinitions[choiceIndex];

                // Handle quest acceptance if this specific chosen next dialogue is marked to accept the quest
                if (chosenNextDialogue != null && chosenNextDialogue.acceptsQuest)
                {
                    if (questToGive != null && questToGive.currentState == QuestData.QuestState.NotStarted)
                    {
                        QuestManager.Instance.AssignQuest(questToGive);
                        Debug.Log($"Quest '{questToGive.questName}' accepted via choice from '{dialogueThatOfferedChoice.name}' leading to '{chosenNextDialogue.name}'!");
                    }
                    else
                    {
                        Debug.LogWarning($"Attempted to accept quest '{questToGive?.questName}' but it's not in NotStarted state or questToGive is null.");
                    }
                }

                // does NOT end the dialogue after a choice, and if there's a next dialogue to play.
                if (chosenNextDialogue != null && !dialogueThatOfferedChoice.endsDialogueAfterChoice)
                {
                    Debug.Log($"Starting next dialogue from choice: {chosenNextDialogue.name}");
                    DialogueManager.Instance.StartDialogue(
                        chosenNextDialogue.dialogueLines,
                        chosenNextDialogue.speakerNames,
                        chosenNextDialogue // Pass the definition itself
                    );
                }
                // If the current dialogue (the one that offered choices) is set to end after a choice,
                // then explicitly end the dialogue.
                else if (dialogueThatOfferedChoice.endsDialogueAfterChoice)
                {
                    Debug.Log($"Choice {choiceIndex} made from '{dialogueThatOfferedChoice.name}' and it ends the dialogue.");
                    DialogueManager.Instance.EndDialogue();
                }
                else // This handles cases where chosenNextDialogue is null (no dialogue linked to choice)
                {
                    Debug.Log($"Choice {choiceIndex} made from '{dialogueThatOfferedChoice.name}', but no subsequent dialogue was linked or it ends the dialogue.");
                    DialogueManager.Instance.EndDialogue(); // End dialogue if no next dialogue is specified for the choice.
                }
            }
            else
            {
                Debug.LogWarning($"Choice index {choiceIndex} out of bounds for '{dialogueThatOfferedChoice.name}'s nextDialogueDefinitions. Ending dialogue.");
                DialogueManager.Instance.EndDialogue(); // End dialogue if choice index is invalid
            }
        }
        else
        {
            Debug.LogWarning("OnDialogueChoiceMadeCallback received, but DialogueManager.Instance.GetCurrentActiveDialogueDefinition() is null. Ending dialogue.");
            DialogueManager.Instance.EndDialogue(); // End dialogue as there's no active definition
        }
    }


    private void UpdateInteractionDefinition()
    {
        DialogueInteractionDefinition definitionToUse = introDialogue; // Default

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
                        // Check if the current quest stage requires returning to *this* giver
                        if (currentStage.requiresReturnToGiver && currentStage.objectiveTargetID == this.name)
                        {
                            if (questToGive.IsCurrentStageComplete())
                            {
                                definitionToUse = questStageCompleteDialogue; // Player has met stage requirements, ready to turn in
                            }
                            else
                            {
                                definitionToUse = questOngoingDialogue; // Player needs to complete objectives for this return stage
                            }
                        }
                        else // Not a 'return to giver' stage for THIS giver
                        {
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

        // Handle cases where a specific dialogue definition might be null
        if (definitionToUse == null)
        {
            Debug.LogWarning($"QuestGiver on {gameObject.name} has a null dialogue definition assigned for state {questToGive?.currentState}. Falling back to introDialogue.");
            definitionToUse = introDialogue; // Fallback to intro if nothing else
            if (definitionToUse == null) // If intro is also null
            {
                Debug.LogWarning($"QuestGiver on {gameObject.name} has NO valid dialogue definitions assigned. Interaction prompt set to 'Interact'.");
                InteractionPromptText = "Interact";
                return;
            }
        }

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
            return false; // Cannot interact if dialogue is already active
        }
        return true; // Can interact if no dialogue is active
    }

    public void Interact(PlayerStateMachine player)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager not found in scene!");
            return;
        }

        DialogueInteractionDefinition dialogueToStart = null;

        if (questToGive != null)
        {
            // Pre-interaction logic: Check if a stage can be completed via this interaction
            if (questToGive.currentState == QuestData.QuestState.Active)
            {
                QuestData.QuestStage currentStage = questToGive.GetCurrentStage();
                if (currentStage != null && currentStage.requiresReturnToGiver && currentStage.objectiveTargetID == this.name)
                {
                    // This is the key change for TalkToNPC stages or return stages
                    // If it's a TalkToNPC stage (or any return stage) and the player is interacting with the correct NPC,
                    // we should consider this interaction as completing the stage, regardless of prior progress.
                    // The objective for a TalkToNPC stage is usually targetAmount = 1.
                    // We directly advance progress here.
                    if (currentStage.stageType == QuestData.QuestType.TalkToNPC || questToGive.IsCurrentStageComplete())
                    {
                        Debug.Log($"Player interacting to complete stage '{currentStage.stageName}' for quest '{questToGive.questName}'. Advancing quest.");
                        QuestManager.Instance.UpdateQuestProgress(questToGive.questName, this.name, 1); // Mark stage complete

                        // NOW, based on the *new* state of the quest, decide the dialogue to play
                        if (questToGive.currentState == QuestData.QuestState.Completed)
                        {
                            dialogueToStart = questCompletedDialogue;
                            QuestManager.Instance.ApplyQuestRewards(questToGive);
                        }
                        else // Quest is active, meaning it advanced to a new stage
                        {
                            dialogueToStart = currentStage.dialogueForStageCompletion ?? questStageCompleteDialogue;
                        }
                    }
                    else
                    {
                        // Player interacted, but a return stage (not TalkToNPC) is NOT complete yet (e.g., gathering, elimination stage before returning)
                        dialogueToStart = questOngoingDialogue;
                    }
                }
            }

            // If no specific dialogue was chosen by the 'return to giver' logic,
            // fall back to the general state-based dialogue.
            if (dialogueToStart == null)
            {
                switch (questToGive.currentState)
                {
                    case QuestData.QuestState.NotStarted:
                        dialogueToStart = introDialogue;
                        break;
                    case QuestData.QuestState.Active:
                        // If it's active but not a return-to-giver type for this NPC
                        dialogueToStart = questOngoingDialogue;
                        break;
                    case QuestData.QuestState.Completed:
                        dialogueToStart = questCompletedDialogue;
                        break;
                    case QuestData.QuestState.Failed:
                        dialogueToStart = questAlreadyActiveDialogue;
                        break;
                }
            }
        }
        else // No questToGive assigned
        {
            dialogueToStart = introDialogue; // Or a generic NPC dialogue
        }

        // Fallback if dialogueToStart is still null
        if (dialogueToStart == null)
        {
            Debug.LogWarning($"QuestGiver on {gameObject.name} has no valid dialogue assigned for current state. Interaction will do nothing.");
            return;
        }

        // Now, start the determined dialogue
        DialogueManager.Instance.StartDialogue(
            dialogueToStart.dialogueLines,
            dialogueToStart.speakerNames,
            dialogueToStart
        );

        // After starting dialogue, ensure the prompt is updated for the *next* interaction
        UpdateInteractionDefinition();
    }

    public string GetInteractionPrompt()
    {
        return InteractionPromptText;
    }
}
