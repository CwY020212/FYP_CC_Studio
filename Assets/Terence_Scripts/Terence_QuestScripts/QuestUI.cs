using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    public GameObject questPanel; // Parent UI panel for displaying quests
    public GameObject completedQuestPanel;
    public GameObject questLogEntryPrefab; // A UI prefab for a single quest entry

    private List<GameObject> currentQuestEntries = new List<GameObject>();
    private List<GameObject> completedQuestEntries = new List<GameObject>();

    private void OnEnable()
    {
        QuestManager.onQuestStarted += AddQuestEntry;
        QuestManager.onQuestProgressChanged += UpdateQuestEntry;
        QuestManager.onQuestCompleted += HandleQuestCompletionUI;
        QuestManager.onShowQuestUI += OnShowQuestUIHandler;
        QuestManager.onQuestStageCompleted += HandleQuestStageCompletionUI; // NEW: Listen to stage completion
    }

    private void OnDisable()
    {
        QuestManager.onQuestStarted -= AddQuestEntry;
        QuestManager.onQuestProgressChanged -= UpdateQuestEntry;
        QuestManager.onQuestCompleted -= HandleQuestCompletionUI;
        QuestManager.onShowQuestUI -= OnShowQuestUIHandler;
        QuestManager.onQuestStageCompleted -= HandleQuestStageCompletionUI;
    }

    void Start()
    {
        RefreshUI();
        ShowActiveQuests();
    }

    private void OnShowQuestUIHandler(QuestData quest)
    {
        Debug.Log($"Received instruction to show UI for quest: {quest.questName}");
        ShowActiveQuests();
        // Potentially highlight the specific quest, if your UI supports it
    }

    void RefreshUI()
    {
        // Clear existing active entries
        foreach (GameObject entry in currentQuestEntries)
        {
            Destroy(entry);
        }
        currentQuestEntries.Clear();

        // Clear existing completed entries
        foreach (GameObject entry in completedQuestEntries)
        {
            Destroy(entry);
        }
        completedQuestEntries.Clear();

        // Add active quests
        if (QuestManager.Instance != null)
        {
            foreach (QuestData quest in QuestManager.Instance.GetActiveQuests())
            {
                AddQuestEntry(quest);
            }
            // Add completed quests on refresh
            foreach (QuestData quest in QuestManager.Instance.GetCompletedQuests())
            {
                AddCompletedQuestEntry(quest);
            }
        }
    }

    void AddQuestEntry(QuestData quest)
    {
        GameObject newEntry = Instantiate(questLogEntryPrefab, questPanel.transform);
        TextMeshProUGUI questNameText = newEntry.transform.Find("Game_QuestNameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI questProgressText = newEntry.transform.Find("Game_QuestProgressText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI questStageText = newEntry.transform.Find("Game_QuestStageText")?.GetComponent<TextMeshProUGUI>(); // Optional: for stage name

        if (questNameText != null)
        {
            questNameText.text = quest.questName;
        }
        else
        {
            Debug.LogWarning("QuestNameText TextMeshProUGUI component not found on prefab!");
        }

        UpdateQuestEntryDisplay(quest, questProgressText, questStageText);

        currentQuestEntries.Add(newEntry);
    }

    void UpdateQuestEntry(QuestData quest)
    {
        foreach (GameObject entry in currentQuestEntries)
        {
            TextMeshProUGUI nameText = entry.transform.Find("Game_QuestNameText").GetComponent<TextMeshProUGUI>();

            if (nameText != null && nameText.text == quest.questName)
            {
                TextMeshProUGUI progressText = entry.transform.Find("Game_QuestProgressText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI stageText = entry.transform.Find("Game_QuestStageText")?.GetComponent<TextMeshProUGUI>(); // Optional

                UpdateQuestEntryDisplay(quest, progressText, stageText);
                break;
            }
        }
    }

    // Helper method to update the display for both Add and Update
    void UpdateQuestEntryDisplay(QuestData quest, TextMeshProUGUI progressText, TextMeshProUGUI stageText)
    {
        QuestData.QuestStage currentStage = quest.GetCurrentStage();
        if (currentStage != null)
        {
            if (progressText != null)
            {
                progressText.text = $"{quest.currentProgressInStage}/{currentStage.targetAmount} {currentStage.stageDescription}";
            }
            else
            {
                Debug.LogWarning("QuestProgressText TextMeshProUGUI component not found for active quest entry!");
            }

            if (stageText != null) // Update stage name if component exists
            {
                stageText.text = currentStage.stageName;
            }
        }
        else
        {
            // Handle case where no stages are defined or currentStage is null
            if (progressText != null) progressText.text = "No active stage.";
            if (stageText != null) stageText.text = "";
        }
    }

    void HandleQuestStageCompletionUI(QuestData quest)
    {
        Debug.Log($"UI: Quest Stage Completed for {quest.questName}. Refreshing active quests UI.");
        // When a stage completes, we just need to update the display of the active quest.
        // The quest remains in 'activeQuests' list in QuestManager until all stages are done.
        UpdateQuestEntry(quest);

        // If the *next* stage is a "return to giver" stage, you might want to show a notification
        // or highlight something on the map.
        QuestData.QuestStage nextStage = quest.GetCurrentStage();
        if (nextStage != null && nextStage.requiresReturnToGiver)
        {
            Debug.Log($"UI: Next stage for {quest.questName} requires returning to the quest giver ({nextStage.objectiveTargetID}).");
            // Optionally, display a pop-up here: "Return to [Quest Giver Name]!"
            // You could also notify a Map/Minimap UI to show an indicator.
            StartCoroutine(ShowReturnToGiverNotification(nextStage.objectiveTargetID));
        }
    }

    void HandleQuestCompletionUI(QuestData quest)
    {
        // Remove from Active Quests UI
        RemoveQuestEntry(quest);

        // Add to Completed Quests UI
        AddCompletedQuestEntry(quest);

        // Immediately show the completed quests panel if desired
        ShowCompletedQuests();
    }

    void AddCompletedQuestEntry(QuestData quest)
    {
        if (completedQuestPanel == null)
        {
            Debug.LogWarning("Completed Quest Panel is not assigned in QuestUI. Completed quests will not be displayed.");
            return;
        }

        GameObject newEntry = Instantiate(questLogEntryPrefab, completedQuestPanel.transform);
        TextMeshProUGUI questNameText = newEntry.transform.Find("Game_QuestNameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI questProgressText = newEntry.transform.Find("Game_QuestProgressText").GetComponent<TextMeshProUGUI>();

        if (questNameText != null)
        {
            questNameText.text = quest.questName;
        }
        if (questProgressText != null)
        {
            questProgressText.text = "COMPLETED!";
        }

        completedQuestEntries.Add(newEntry);
    }

    void RemoveQuestEntry(QuestData quest)
    {
        GameObject entryToRemove = null;
        foreach (GameObject entry in currentQuestEntries)
        {
            TextMeshProUGUI nameText = entry.transform.Find("Game_QuestNameText").GetComponent<TextMeshProUGUI>();
            if (nameText != null && nameText.text == quest.questName)
            {
                entryToRemove = entry;
                break;
            }
        }

        if (entryToRemove != null)
        {
            currentQuestEntries.Remove(entryToRemove);
            Destroy(entryToRemove);
        }
    }

    public void ShowActiveQuests()
    {
        questPanel.SetActive(true);
        if (completedQuestPanel != null)
        {
            completedQuestPanel.SetActive(false);
        }
    }

    public void ShowCompletedQuests()
    {
        if (completedQuestPanel != null)
        {
            completedQuestPanel.SetActive(true);
            questPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Completed Quest Panel is not assigned!");
        }
    }

    // Example notification coroutine
    private IEnumerator ShowReturnToGiverNotification(string giverName)
    {
        Debug.Log($"Displaying 'Return to {giverName}!' notification for 3 seconds.");
        // Implement actual UI pop-up logic here (e.g., enable a temporary TextMeshPro object)
        yield return new WaitForSeconds(3f);
        Debug.Log("Notification hidden.");
        // Hide notification UI
    }
}