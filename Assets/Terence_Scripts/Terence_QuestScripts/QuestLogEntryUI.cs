using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class QuestLogEntryUI : MonoBehaviour
{
    public TextMeshPro questNameText;
    public Image trackedIndicator; // An Image component to show if quest is tracked
    public Button button; // The button that makes this whole entry clickable

    private Quest _quest;

    public void Setup(Quest quest)
    {
        _quest = quest;
        questNameText.text = quest.questName;
        // Optionally change color based on state, e.g., gray if completed
        if (quest.currentState == QuestState.Completed)
        {
            questNameText.color = Color.grey;
        }
        else
        {
            questNameText.color = Color.white;
        }

        // Ensure button is assigned
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    public void SetTrackedIndicator(bool isTracked)
    {
        if (trackedIndicator != null)
        {
            trackedIndicator.gameObject.SetActive(isTracked);
        }
    }
}
