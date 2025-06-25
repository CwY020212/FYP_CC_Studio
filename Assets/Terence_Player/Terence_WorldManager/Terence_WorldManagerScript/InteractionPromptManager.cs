using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InteractionPromptManager : MonoBehaviour
{
    public static InteractionPromptManager Instance { get; private set; }

    [SerializeField] private GameObject defaultPromptPrefab; // Your UI prefab (should contain a World Space Canvas)
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 0.5f, 0); // Offset above the object in world space

    private GameObject currentPromptInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public GameObject ShowPrompt(IInteractable interactable, Vector3 worldPosition)
    {
        // If a new interactable, or no prompt exists, create/reposition
        if (currentPromptInstance == null)
        {
            // Instantiate the prompt prefab directly at the world position
            // Make sure your prefab itself has a World Space Canvas and UI elements
            currentPromptInstance = Instantiate(defaultPromptPrefab, worldPosition + promptOffset, Quaternion.identity);
            // Optionally, make it a child of the interactable for easy cleanup/positioning
            // currentPromptInstance.transform.SetParent(((MonoBehaviour)interactable).transform);
            // currentPromptInstance.transform.localPosition = promptOffset;

            // Update text if it exists within the prefab
            TextMeshProUGUI promptText = currentPromptInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (promptText != null)
            {
                promptText.text = interactable.GetInteractionPrompt();
            }
        }
        else
        {
            // Just reposition if already exists
            currentPromptInstance.transform.position = worldPosition + promptOffset;
        }

        currentPromptInstance.SetActive(true);
        return currentPromptInstance;
    }

    public void HidePrompt(IInteractable interactable)
    {
        if (currentPromptInstance != null && currentPromptInstance.activeSelf)
        {
            currentPromptInstance.SetActive(false);
            // If you always want to destroy, you can do that here:
            // Destroy(currentPromptInstance);
            // currentPromptInstance = null;
        }
    }
}
