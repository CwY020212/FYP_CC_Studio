using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestTrigger : MonoBehaviour
{
    public string questID;
    public string objectiveID;
    public ObjectiveType objectiveTriggerType; // e.g., GoToLocation, SolvePuzzle

    public bool destroyAfterUse = true; // Destroy trigger after it activates

    private bool _hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;

        // Ensure this is the player character (adjust tag as needed)
        if (other.CompareTag("Player"))
        {
            Quest activeQuest = QuestManager.Instance.ActiveQuests.Find(q => q.questID == questID);

            if (activeQuest != null && activeQuest.currentState == QuestState.Active)
            {
                QuestObjective objective = activeQuest.objectives.Find(o => o.objectiveID == objectiveID && o.type == objectiveTriggerType);

                if (objective != null && !objective.isCompleted)
                {
                    Debug.Log($"Quest Trigger Activated: {objective.objectiveName}");
                    QuestManager.Instance.CompleteObjectiveDirectly(questID, objectiveID);
                    _hasTriggered = true; // Prevent multiple triggers

                    if (destroyAfterUse)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }

    // Visualize the trigger in editor
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            if (col is BoxCollider box)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
            }
            // Add other collider types as needed
            Gizmos.color = new Color(0, 1, 0, 1f);
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f); // Small indicator
        }
    }
}