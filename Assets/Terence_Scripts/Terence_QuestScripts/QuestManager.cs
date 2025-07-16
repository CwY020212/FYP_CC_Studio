using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // For .FirstOrDefault()

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Available Quests")]
    public List<QuestData> availableQuests = new List<QuestData>();

    private List<QuestData> activeQuests = new List<QuestData>();
    private List<QuestData> completedQuests = new List<QuestData>();

    // Events to notify other systems (e.g., UI)
    public delegate void OnQuestStarted(QuestData quest);
    public static event OnQuestStarted onQuestStarted;

    public delegate void OnShowQuestUI(QuestData quest);
    public static event OnShowQuestUI onShowQuestUI;

    public delegate void OnQuestProgressChanged(QuestData quest);
    public static event OnQuestProgressChanged onQuestProgressChanged;

    public delegate void OnQuestCompleted(QuestData quest);
    public static event OnQuestCompleted onQuestCompleted;

    // NEW EVENT: For when a quest stage is completed (but the quest itself continues)
    public delegate void OnQuestStageCompleted(QuestData quest);
    public static event OnQuestStageCompleted onQuestStageCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (QuestData quest in availableQuests)
        {
            quest.InitializeQuest();
        }
    }

    public void AssignQuest(QuestData questToStart)
    {
        if (questToStart == null)
        {
            Debug.LogError("Attempted to assign a null quest.");
            return;
        }

        if (questToStart.currentState == QuestData.QuestState.NotStarted)
        {
            questToStart.StartQuest();
            activeQuests.Add(questToStart);
            availableQuests.Remove(questToStart);
            onQuestStarted?.Invoke(questToStart);
            Debug.Log($"Assigned quest: {questToStart.questName}");
        }
        else
        {
            Debug.LogWarning($"Quest '{questToStart.questName}' is already {questToStart.currentState}.");
        }
    }

    /// <summary>
    /// Updates the progress of an active quest's current stage.
    /// </summary>
    /// <param name="questName">The name of the quest to update.</param>
    /// <param name="objectiveID">The specific objective ID for the current stage (e.g., "WolfPelts").</param>
    /// <param name="amount">The amount to add to the quest's current progress for the stage.</param>
    public void UpdateQuestProgress(string questName, string objectiveID, int amount = 1)
    {
        QuestData quest = activeQuests.FirstOrDefault(q => q.questName == questName);

        if (quest != null)
        {
            // QuestData.AdvanceProgress will now handle checking if the stage is complete
            int previousStageIndex = quest.currentStageIndex;
            quest.AdvanceProgress(objectiveID, amount);

            // If a stage was completed or progress changed
            if (quest.currentStageIndex > previousStageIndex)
            {
                // A new stage has begun, notify for UI update
                onQuestStageCompleted?.Invoke(quest); // New event for stage completion
            }
            else if (quest.currentState == QuestData.QuestState.Active) // Only invoke if quest is still active (not fully completed yet)
            {
                onQuestProgressChanged?.Invoke(quest); // Still invoke for general progress updates within a stage
            }

            if (quest.IsComplete())
            {
                activeQuests.Remove(quest);
                completedQuests.Add(quest);
                onQuestCompleted?.Invoke(quest); // This event is only for full quest completion
                ApplyQuestRewards(quest);
            }
        }
        else
        {
            Debug.LogWarning($"Quest '{questName}' not found or not active.");
        }
    }

    public void NotifyShowQuestUI(QuestData quest)
    {
        onShowQuestUI?.Invoke(quest);
    }

    // NEW METHOD: For notifying when a quest stage is completed, so UI can react.
    public void NotifyQuestStageCompleted(QuestData quest)
    {
        onQuestStageCompleted?.Invoke(quest);
    }

    private void ApplyQuestRewards(QuestData quest)
    {
        Debug.Log($"Applying rewards for '{quest.questName}': {quest.questReward.experience} XP, {quest.questReward.gold} Gold.");
    }

    public List<QuestData> GetActiveQuests()
    {
        return activeQuests;
    }

    public List<QuestData> GetCompletedQuests()
    {
        return completedQuests;
    }

    // NEW: Get an active quest by name, useful for QuestGiver
    public QuestData GetActiveQuest(string questName)
    {
        return activeQuests.FirstOrDefault(q => q.questName == questName);
    }
}