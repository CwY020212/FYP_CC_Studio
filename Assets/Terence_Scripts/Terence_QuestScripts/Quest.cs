using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/New Quest", order = 1)]
public class Quest : ScriptableObject
{
    public string questID; // Unique identifier for the quest (e.g., "HR_WhispersOfLostHeir")
    public string questName; // Display name (e.g., "Whispers of the Lost Heir")
    [TextArea(3, 10)]
    public string questDescription; // Full description for the quest log
    [TextArea(3, 10)]
    public string completionDialogue; // Dialogue from QuestGiver upon completion

    public QuestState currentState = QuestState.NotStarted;

    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Rewards")]
    //public Ability unlockedAbility; // Reference to an ability ScriptableObject (you'd need to create this)
    public List<GameObject> rewardItems; // List of items to give the player

    // Optional: Reference to the next quest in a chain
    public Quest nextQuestInChain;

    public QuestObjective GetCurrentActiveObjective()
    {
        foreach (QuestObjective obj in objectives)
        {
            if (!obj.isCompleted)
            {
                return obj;
            }
        }
        return null; // All objectives completed or no objectives defined
    }

    public bool AreAllObjectivesCompleted()
    {
        foreach (QuestObjective obj in objectives)
        {
            if (!obj.isCompleted)
            {
                return false;
            }
        }
        return true;
    }

    // Call this if the quest needs to be reset for replayability or failure states
    public void ResetQuest()
    {
        currentState = QuestState.NotStarted;
        foreach (QuestObjective obj in objectives)
        {
            obj.ResetProgress();
        }
    }
}

