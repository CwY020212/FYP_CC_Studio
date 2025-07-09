using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // For .FirstOrDefault()

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Available Quests (Drag all Quest ScriptableObjects here)")]
    public List<Quest> allAvailableQuests = new List<Quest>();

    private List<Quest> _activeQuests = new List<Quest>();
    public List<Quest> ActiveQuests => _activeQuests;

    private Quest _trackedQuest; // The quest the player has chosen to follow
    public Quest TrackedQuest => _trackedQuest;

    // Events for UI and other systems to subscribe to
    public static event Action<Quest> OnQuestStarted;
    public static event Action<Quest, QuestObjective> OnObjectiveProgressed;
    public static event Action<Quest, QuestObjective> OnObjectiveCompleted;
    public static event Action<Quest> OnQuestCompleted;
    public static event Action<Quest> OnTrackedQuestChanged;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes if needed
        }

        // Initialize all quests to NotStarted state
        foreach (var quest in allAvailableQuests)
        {
            quest.currentState = QuestState.NotStarted;
            foreach (var obj in quest.objectives)
            {
                obj.ResetProgress(); // Ensure objectives are reset
            }
        }
    }

    /// <summary>
    /// Starts a quest and adds it to the active quests list.
    /// </summary>
    /// <param name="questID">The unique ID of the quest to start.</param>
    public void StartQuest(string questID)
    {
        Quest quest = allAvailableQuests.FirstOrDefault(q => q.questID == questID);
        if (quest == null)
        {
            Debug.LogWarning($"Quest with ID '{questID}' not found!");
            return;
        }

        if (quest.currentState == QuestState.NotStarted)
        {
            quest.currentState = QuestState.Active;
            _activeQuests.Add(quest);
            Debug.Log($"Quest Started: {quest.questName}");
            OnQuestStarted?.Invoke(quest);

            // Automatically track the first quest started if none is tracked
            if (_trackedQuest == null)
            {
                SetTrackedQuest(questID);
            }
        }
        else
        {
            Debug.Log($"Quest '{quest.questName}' is already active or completed.");
        }
    }

    /// <summary>
    /// Progresses a specific objective within a quest.
    /// </summary>
    /// <param name="questID">The ID of the quest.</param>
    /// <param name="objectiveID">The ID of the objective.</param>
    /// <param name="amount">Amount to progress by (default 1).</param>
    public void ProgressObjective(string questID, string objectiveID, int amount = 1)
    {
        Quest quest = _activeQuests.FirstOrDefault(q => q.questID == questID);
        if (quest == null || quest.currentState != QuestState.Active)
        {
            // Debug.LogWarning($"Cannot progress objective. Quest '{questID}' not active or found.");
            return;
        }

        QuestObjective objective = quest.objectives.FirstOrDefault(o => o.objectiveID == objectiveID);
        if (objective == null)
        {
            Debug.LogWarning($"Objective with ID '{objectiveID}' not found in Quest '{questID}'.");
            return;
        }

        if (!objective.isCompleted)
        {
            objective.Progress(amount);
            Debug.Log($"Objective Progressed: {objective.objectiveName} ({objective.currentAmount}/{objective.requiredAmount})");
            OnObjectiveProgressed?.Invoke(quest, objective);

            if (objective.CheckCompletion())
            {
                objective.isCompleted = true; // Ensure isCompleted is set
                Debug.Log($"Objective Completed: {objective.objectiveName}");
                OnObjectiveCompleted?.Invoke(quest, objective);
                CheckQuestCompletion(quest);
            }
        }
    }

    /// <summary>
    /// Marks a specific objective as completed directly (e.g., for TalkToNPC, GoToLocation, SolvePuzzle).
    /// </summary>
    public void CompleteObjectiveDirectly(string questID, string objectiveID)
    {
        Quest quest = _activeQuests.FirstOrDefault(q => q.questID == questID);
        if (quest == null || quest.currentState != QuestState.Active)
        {
            // Debug.LogWarning($"Cannot complete objective directly. Quest '{questID}' not active or found.");
            return;
        }

        QuestObjective objective = quest.objectives.FirstOrDefault(o => o.objectiveID == objectiveID);
        if (objective == null)
        {
            Debug.LogWarning($"Objective with ID '{objectiveID}' not found in Quest '{questID}'.");
            return;
        }

        if (!objective.isCompleted)
        {
            objective.isCompleted = true;
            Debug.Log($"Objective Completed Directly: {objective.objectiveName}");
            OnObjectiveCompleted?.Invoke(quest, objective);
            CheckQuestCompletion(quest);
        }
    }


    private void CheckQuestCompletion(Quest quest)
    {
        if (quest.AreAllObjectivesCompleted())
        {
            quest.currentState = QuestState.Completed;
            _activeQuests.Remove(quest);
            Debug.Log($"Quest Completed: {quest.questName}");
            OnQuestCompleted?.Invoke(quest);

            // Optional: Start next quest in chain
            if (quest.nextQuestInChain != null)
            {
                StartQuest(quest.nextQuestInChain.questID);
            }

            // If the completed quest was the tracked quest, untrack it
            if (_trackedQuest == quest)
            {
                SetTrackedQuest(null); // Or set to another active quest
            }
        }
    }

    /// <summary>
    /// Sets the currently tracked quest for UI markers, etc.
    /// </summary>
    /// <param name="questID">The ID of the quest to track. Pass null to untrack.</param>
    public void SetTrackedQuest(string questID)
    {
        if (string.IsNullOrEmpty(questID))
        {
            _trackedQuest = null;
            Debug.Log("Untracking quest.");
            OnTrackedQuestChanged?.Invoke(null);
            return;
        }

        Quest questToTrack = _activeQuests.FirstOrDefault(q => q.questID == questID);
        if (questToTrack != null && questToTrack.currentState == QuestState.Active)
        {
            _trackedQuest = questToTrack;
            Debug.Log($"Tracking Quest: {_trackedQuest.questName}");
            OnTrackedQuestChanged?.Invoke(_trackedQuest);
        }
        else
        {
            Debug.LogWarning($"Cannot track quest '{questID}'. It's not active or found.");
        }
    }
}