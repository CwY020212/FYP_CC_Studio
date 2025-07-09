using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    public string npcID; // Unique ID for this NPC 

    [Header("Quest Assignment")]
    public Quest questToGive; // Drag the Quest ScriptableObject here

    [Header("Dialogue Lines")]
    [TextArea(3, 5)]
    public string dialogueBeforeQuest; // When quest NotStarted
    [TextArea(3, 5)]
    public string dialogueDuringQuest; // When quest Active
    [TextArea(3, 5)]
    public string dialogueOnCompletion; // When quest completed via this NPC

    [Header("Objective Completion (Optional)")]
    public bool completesObjectiveOnTalk = false;
    public string objectiveIDToComplete; // The ID of the objective this NPC completes directly

    private void Start()
    {
        if (questToGive == null)
        {
            Debug.LogWarning($"QuestGiver {gameObject.name} does not have a Quest assigned!");
        }
    }

    public void OnPlayerInteract() // Call this from your Player Interaction system
    {
        if (questToGive == null)
        {
            Debug.LogWarning($"QuestGiver {gameObject.name} has no quest assigned.");
            return;
        }

        switch (questToGive.currentState)
        {
            case QuestState.NotStarted:
                Debug.Log($"NPC Dialogue: {dialogueBeforeQuest}");
                // In a real game, you'd trigger a dialogue system here
                // For now, we'll auto-start the quest after dialogue
                QuestManager.Instance.StartQuest(questToGive.questID);
                break;

            case QuestState.Active:
                Debug.Log($"NPC Dialogue: {dialogueDuringQuest}");
                if (completesObjectiveOnTalk)
                {
                    QuestManager.Instance.CompleteObjectiveDirectly(questToGive.questID, objectiveIDToComplete);
                }
                break;

            case QuestState.Completed:
                Debug.Log($"NPC Dialogue: {dialogueOnCompletion}");
                // Optionally give rewards again or trigger post-quest content
                break;
        }
    }

    // For debugging/testing interaction without a full player system
    private void OnMouseDown()
    {
        OnPlayerInteract();
    }
}
