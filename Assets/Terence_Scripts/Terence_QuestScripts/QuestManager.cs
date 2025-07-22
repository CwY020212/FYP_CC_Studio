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
            QuestData.QuestState previousQuestState = quest.currentState; // Capture state BEFORE progress update
            int previousStageIndex = quest.currentStageIndex;

            // This call will internally complete the current stage and advance currentStageIndex
            // It will also potentially change quest.currentState to Completed
            quest.AdvanceProgress(objectiveID, amount);

            // --- REFINED EVENT INVocation LOGIC ---

            // Check if a stage was completed (index changed OR quest moved from Active to Completed)
            if (quest.currentStageIndex > previousStageIndex ||
                (previousQuestState == QuestData.QuestState.Active && quest.currentState == QuestData.QuestState.Completed))
            {
                // A stage has been completed (either moved to next stage or finished the entire quest)
                onQuestStageCompleted?.Invoke(quest);
            }
            else if (quest.currentState == QuestData.QuestState.Active)
            {
                // Progress updated within the *same* stage
                onQuestProgressChanged?.Invoke(quest);
            }

            // Check for overall quest completion (this should always be the final check)
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

    public void ApplyQuestRewards(QuestData quest)
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