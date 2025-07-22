using Mono.Cecil.Cil;
using System.Buffers.Text;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class QuestTrigger : MonoBehaviour, IInteractable
{
    [Header("Quest Integration")]
    public string relevantQuestName; // The name of the quest this item contributes to
    public string objectiveID; // The specific objective ID for collecting this item (e.g., "Wood")

    [Header("Interaction Settings")]
    public string interactionPrompt = "Collect Wood"; // Text to show when player is near
    public bool destroyOnCollect = true; // Whether the wood object disappears after collection

    // These are part of the IInteractable interface and will be set by PlayerInteractionController
    public string InteractionPromptText { get; private set; }
    public GameObject CurrentWorldSpacePrompt { get; set; } // This now holds the reference to the prompt managed by InteractionPromptManager

    private void Awake()
    {
        InteractionPromptText = interactionPrompt;
    }

    private void OnEnable()
    {
        // For example, if you have a system that shows interaction prompts
        // You might register this object with an InteractionManager here
    }

    private void OnDisable()
    {
        // IMPORTANT: If this object is destroyed or disabled, ensure its prompt is also hidden.
        // This is crucial for cleanup if the object is destroyed outside of the Interact method.
        if (InteractionPromptManager.Instance != null && CurrentWorldSpacePrompt != null)
        {
            InteractionPromptManager.Instance.HidePrompt(this); // Pass 'this' as the IInteractable
        }
    }

    public string GetInteractionPrompt()
    {
        // You could make this dynamic, e.g., "Collect Wood (2/3)"
        // For example:
        // if (QuestManager.Instance != null && QuestManager.Instance.GetActiveQuest(relevantQuestName) != null)
        // {
        //     QuestData quest = QuestManager.Instance.GetActiveQuest(relevantQuestName);
        //     if (quest != null && quest.GetCurrentStage() != null && quest.GetCurrentStage().objectiveTargetID == objectiveID)
        //     {
        //         return $"{interactionPrompt} ({quest.GetCurrentStage().currentProgress}/{quest.GetCurrentStage().requiredAmount})";
        //     }
        // }
        return InteractionPromptText;
    }

    public bool CanInteract(PlayerStateMachine player)
    {
        if (QuestManager.Instance == null) return false;

        QuestData quest = QuestManager.Instance.GetActiveQuest(relevantQuestName);
        if (quest == null || quest.currentState != QuestData.QuestState.Active)
        {
            // Only interact if the relevant quest is active
            return false;
        }

        // Check if the current stage requires this objectiveID
        QuestData.QuestStage currentStage = quest.GetCurrentStage();
        if (currentStage == null || currentStage.objectiveTargetID != objectiveID || quest.IsCurrentStageComplete())
        {
            // Don't interact if this isn't the correct objective for the current stage,
            // or if the current stage is already complete.
            return false;
        }

        return true; // Allow interaction if conditions met
    }

    public void Interact(PlayerStateMachine player)
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager not found! Cannot collect wood.");
            return;
        }

        QuestData quest = QuestManager.Instance.GetActiveQuest(relevantQuestName);

        if (quest != null && quest.currentState == QuestData.QuestState.Active)
        {
            QuestData.QuestStage currentStage = quest.GetCurrentStage();

            // Check if this interaction is relevant to the current quest stage
            if (currentStage != null && currentStage.objectiveTargetID == objectiveID && !quest.IsCurrentStageComplete())
            {
                // Call QuestManager to advance progress for this objective
                QuestManager.Instance.UpdateQuestProgress(relevantQuestName, objectiveID, 1);

                // Optional: Play a sound effect or animation for collection
                Debug.Log($"Collected 1 {objectiveID} for quest: {relevantQuestName}");

                if (destroyOnCollect)
                {
                    // Hide the prompt through the manager BEFORE destroying the GameObject.
                    // The manager will then handle disabling/destroying the UI element.
                    if (InteractionPromptManager.Instance != null)
                    {
                        InteractionPromptManager.Instance.HidePrompt(this); // Pass 'this' as the IInteractable
                    }
                    else
                    {
                        Debug.LogWarning("InteractionPromptManager.Instance is null when trying to hide prompt for destroyed object.");
                    }

                    // Now, destroy the wood object itself
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log($"Collected {objectiveID}, but it's not needed for the current quest stage or stage is complete.");
                // Optionally, don't destroy or just remove from world without affecting quest
            }
        }
        else
        {
            Debug.Log("No active quest to collect wood for.");
        }
    }
}