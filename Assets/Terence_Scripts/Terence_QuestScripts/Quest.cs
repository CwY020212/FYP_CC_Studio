using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea(3, 10)]
    public string questDescription;
    public QuestType questType;
    // Removed targetAmount and objectives from top level, they will be per stage

    public QuestRewardData questReward;

    // New: Array of Quest Stages
    public QuestStage[] questStages;

    // Runtime state variables
    [System.NonSerialized]
    public QuestState currentState = QuestState.NotStarted;
    [System.NonSerialized]
    public int currentStageIndex = 0; // Tracks the current active stage
    [System.NonSerialized]
    public int currentProgressInStage = 0; // Tracks progress within the current stage

    public enum QuestType
    {
        Gathering,
        Elimination,
        Delivery,
        Exploration,
        TalkToNPC // New type for "find the quest giver" stage
    }

    public enum QuestState
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    [System.Serializable]
    public class QuestStage
    {
        public string stageName; // e.g., "Collect 5 Wolf Pelts", "Return to Giver"
        [TextArea(2, 5)]
        public string stageDescription;
        public QuestType stageType; // Type for this specific stage
        public int targetAmount; // Target for this stage (e.g., 5 pelts, 1 for TalkToNPC)
        public string objectiveTargetID; // e.g., "WolfPelts", "QuestGiverName" for TalkToNPC
        public DialogueInteractionDefinition dialogueForStageCompletion; // Dialogue after this stage is done but before quest is complete
        public bool requiresReturnToGiver; // If true, this stage means "go talk to the original quest giver"
    }

    public void InitializeQuest()
    {
        currentState = QuestState.NotStarted;
        currentStageIndex = 0;
        currentProgressInStage = 0;
    }

    public void StartQuest()
    {
        if (currentState == QuestState.NotStarted)
        {
            currentState = QuestState.Active;
            currentStageIndex = 0;
            currentProgressInStage = 0;
            Debug.Log($"Quest '{questName}' started! Current stage: {GetCurrentStage().stageName}");
        }
    }

    public QuestStage GetCurrentStage()
    {
        if (questStages != null && currentStageIndex >= 0 && currentStageIndex < questStages.Length)
        {
            return questStages[currentStageIndex];
        }
        return null;
    }

    public void AdvanceProgress(string objectiveID, int amount = 1)
    {
        if (currentState != QuestState.Active) return;

        QuestStage currentStage = GetCurrentStage();
        if (currentStage == null) return;

        // Ensure the objectiveID matches the current stage's target
        if (currentStage.objectiveTargetID != objectiveID)
        {
            Debug.LogWarning($"Progress update for '{objectiveID}' but current stage requires '{currentStage.objectiveTargetID}'.");
            return;
        }

        currentProgressInStage += amount;
        Debug.Log($"Quest '{questName}' - Stage '{currentStage.stageName}' progress: {currentProgressInStage}/{currentStage.targetAmount}");

        if (currentProgressInStage >= currentStage.targetAmount)
        {
            CompleteCurrentStage();
        }
    }

    private void CompleteCurrentStage()
    {
        QuestStage completedStage = GetCurrentStage();
        if (completedStage == null) return;

        Debug.Log($"Quest '{questName}' - Stage '{completedStage.stageName}' completed!");

        if (currentStageIndex < questStages.Length - 1)
        {
            currentStageIndex++; // Move to the next stage
            currentProgressInStage = 0; // Reset progress for the new stage
            currentState = QuestState.Active; // Still active for the next stage
            Debug.Log($"Quest '{questName}' advanced to next stage: {GetCurrentStage().stageName}");

            // Notify UI that a stage has been completed and quest continues
            QuestManager.Instance.NotifyQuestStageCompleted(this);
        }
        else
        {
            // All stages completed, quest is fully done
            currentState = QuestState.Completed;
            Debug.Log($"Quest '{questName}' fully completed! You received {questReward.experience} XP and {questReward.gold} gold.");
        }
    }

    public bool IsComplete()
    {
        return currentState == QuestState.Completed;
    }

    public bool IsCurrentStageComplete()
    {
        QuestStage currentStage = GetCurrentStage();
        return currentStage != null && currentProgressInStage >= currentStage.targetAmount;
    }
}

[System.Serializable]
public class QuestRewardData
{
    public int experience;
    public int gold;
    // Add other reward types like items, abilities, etc.
    // public ItemData[] rewardItems; // Requires an ItemData ScriptableObject or similar
}