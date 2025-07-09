using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class QuestObjective
{
    public string objectiveID; // Unique ID for this objective within its quest
    public string objectiveName; // e.g., "Find the Archivist"
    public ObjectiveType type;
    public bool isCompleted;

    // --- Type-Specific Data ---

    [Header("TalkToNPC")]
    public string targetNpcID; // ID of the NPC to talk to

    [Header("CollectItem")]
    public string targetItemID; // ID of the item to collect
    public int requiredAmount;
    public int currentAmount;

    [Header("DefeatEnemy")]
    public string targetEnemyID; // ID of the enemy to defeat (or a specific boss)
    public int requiredKills;
    public int currentKills;

    [Header("GoToLocation / SolvePuzzle")]
    public Transform targetLocation; // Reference to a GameObject/Transform in the scene
                                     // This is used for quest markers, often a child GameObject
                                     // specifically for marker placement.
    public float proximityRadius = 2f; // For GoToLocation objectives

    public void Progress(int amount = 1)
    {
        if (isCompleted) return;

        switch (type)
        {
            case ObjectiveType.CollectItem:
                currentAmount += amount;
                if (currentAmount >= requiredAmount)
                {
                    isCompleted = true;
                }
                break;
            case ObjectiveType.DefeatEnemy:
                currentKills += amount;
                if (currentKills >= requiredKills)
                {
                    isCompleted = true;
                }
                break;
            case ObjectiveType.TalkToNPC: // For talk objectives, they are usually completed directly by QuestGiver
            case ObjectiveType.GoToLocation: // For location objectives, check is done by player position
            case ObjectiveType.SolvePuzzle: // For puzzle objectives, check is done by puzzle completion trigger
                break;
        }
    }

    public bool CheckCompletion()
    {
        if (isCompleted) return true;

        switch (type)
        {
            case ObjectiveType.CollectItem:
                return currentAmount >= requiredAmount;
            case ObjectiveType.DefeatEnemy:
                return currentKills >= requiredKills;
            case ObjectiveType.TalkToNPC:
            case ObjectiveType.GoToLocation:
            case ObjectiveType.SolvePuzzle:
                // These types are typically marked complete externally by a specific trigger or event
                return isCompleted; // Relies on external setting of isCompleted
        }
        return false;
    }

    public void ResetProgress()
    {
        isCompleted = false;
        currentAmount = 0;
        currentKills = 0;
    }
}
