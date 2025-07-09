using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestLogUI : MonoBehaviour
{
    public GameObject questEntryPrefab; // Prefab for a single quest entry in the log
    public Transform questListContent; // Parent transform for quest entries

    public Text questNameText;
    public Text questDescriptionText;
    public Transform objectiveListContent; // Parent transform for objective entries
    public GameObject objectiveEntryPrefab; // Prefab for a single objective entry

    public GameObject noQuestSelectedPanel; // Panel to show when no quest is selected
    public GameObject questDetailsPanel; // Panel to show quest details

    private Quest _selectedQuest;

    void OnEnable()
    {
        // Subscribe to events
        QuestManager.OnQuestStarted += RefreshQuestList;
        QuestManager.OnObjectiveProgressed += OnObjectiveProgressedHandler;
        QuestManager.OnObjectiveCompleted += OnObjectiveCompletedHandler;
        QuestManager.OnQuestCompleted += RefreshQuestList;
        QuestManager.OnTrackedQuestChanged += RefreshQuestList; // To update tracking UI

        RefreshQuestList(null); // Initial refresh
        SelectQuest(null); // Clear selection initially
    }

    void OnDisable()
    {
        // Unsubscribe to events to prevent memory leaks
        QuestManager.OnQuestStarted -= RefreshQuestList;
        QuestManager.OnObjectiveProgressed -= OnObjectiveProgressedHandler;
        QuestManager.OnObjectiveCompleted -= OnObjectiveCompletedHandler;
        QuestManager.OnQuestCompleted -= RefreshQuestList;
        QuestManager.OnTrackedQuestChanged -= RefreshQuestList;
    }

    void RefreshQuestList(Quest updatedQuest)
    {
        // Clear old entries
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // Populate with active quests
        foreach (Quest quest in QuestManager.Instance.ActiveQuests)
        {
            GameObject entryGO = Instantiate(questEntryPrefab, questListContent);
            QuestLogEntryUI entryUI = entryGO.GetComponent<QuestLogEntryUI>();
            if (entryUI != null)
            {
                entryUI.Setup(quest);
                entryUI.button.onClick.AddListener(() => SelectQuest(quest));

                // Indicate if this quest is tracked
                if (QuestManager.Instance.TrackedQuest == quest)
                {
                    entryUI.SetTrackedIndicator(true);
                }
                else
                {
                    entryUI.SetTrackedIndicator(false);
                }
            }
        }

        // Re-select the currently selected quest to update its details, or clear if it's no longer active
        if (_selectedQuest != null && !QuestManager.Instance.ActiveQuests.Contains(_selectedQuest))
        {
            SelectQuest(null); // Clear selection if it was completed
        }
        else if (_selectedQuest != null)
        {
            SelectQuest(_selectedQuest); // Refresh details for existing selection
        }
    }

    void SelectQuest(Quest quest)
    {
        _selectedQuest = quest;

        if (_selectedQuest == null)
        {
            noQuestSelectedPanel.SetActive(true);
            questDetailsPanel.SetActive(false);
            return;
        }

        noQuestSelectedPanel.SetActive(false);
        questDetailsPanel.SetActive(true);

        questNameText.text = _selectedQuest.questName;
        questDescriptionText.text = _selectedQuest.questDescription;

        // Clear old objective entries
        foreach (Transform child in objectiveListContent)
        {
            Destroy(child.gameObject);
        }

        // Populate objectives
        foreach (QuestObjective obj in _selectedQuest.objectives)
        {
            GameObject objEntryGO = Instantiate(objectiveEntryPrefab, objectiveListContent);
            Text objText = objEntryGO.GetComponent<Text>(); // Or a custom ObjectiveEntryUI script
            if (objText != null)
            {
                string status = obj.isCompleted ? "Yes " : "No ";
                string progress = "";
                if (obj.type == ObjectiveType.CollectItem || obj.type == ObjectiveType.DefeatEnemy)
                {
                    progress = $" ({obj.currentAmount}/{obj.requiredAmount})";
                }
                objText.text = status + obj.objectiveName + progress;
                objText.color = obj.isCompleted ? Color.green : Color.white;
            }
        }
    }

    // Handles objective progress/completion directly impacting the currently selected quest's details
    void OnObjectiveProgressedHandler(Quest quest, QuestObjective objective)
    {
        if (quest == _selectedQuest)
        {
            SelectQuest(_selectedQuest); // Re-render details to show progress
        }
    }

    void OnObjectiveCompletedHandler(Quest quest, QuestObjective objective)
    {
        if (quest == _selectedQuest)
        {
            SelectQuest(_selectedQuest); // Re-render details to show completion
        }
    }

    // Call this from a button in your UI to track/untrack the selected quest
    public void ToggleTrackSelectedQuest()
    {
        if (_selectedQuest == null) return;

        if (QuestManager.Instance.TrackedQuest == _selectedQuest)
        {
            QuestManager.Instance.SetTrackedQuest(null); // Untrack
        }
        else
        {
            QuestManager.Instance.SetTrackedQuest(_selectedQuest.questID); // Track
        }
        RefreshQuestList(null); // Refresh list to update tracked indicator
    }
}
