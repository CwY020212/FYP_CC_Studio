using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InteractionPromptManager : MonoBehaviour
{
    public static InteractionPromptManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject promptPrefab; // A prefab containing a TextMeshProUGUI for the prompt
    public Canvas parentCanvas; // The main UI Canvas for screen-space overlay

    private Dictionary<IInteractable, GameObject> activePrompts = new Dictionary<IInteractable, GameObject>();

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
    }

    /// <summary>
    /// Shows or updates an interaction prompt for a given interactable.
    /// </summary>
    /// <param name="interactable">The interactable requesting the prompt.</param>
    /// <param name="worldPosition">The world position to anchor the prompt (e.g., above NPC's head).</param>
    /// <returns>The instantiated prompt GameObject.</returns>
    public GameObject ShowPrompt(IInteractable interactable, Vector3 worldPosition)
    {
        if (promptPrefab == null || parentCanvas == null)
        {
            Debug.LogError("Prompt Prefab or Parent Canvas not assigned in InteractionPromptManager.");
            return null;
        }

        GameObject promptInstance;
        if (activePrompts.TryGetValue(interactable, out promptInstance))
        {
            // Prompt already exists, just update its text and position
            UpdatePromptText(interactable, interactable.GetInteractionPrompt());
            UpdatePromptPosition(promptInstance, worldPosition);
        }
        else
        {
            // Create new prompt
            promptInstance = Instantiate(promptPrefab, parentCanvas.transform);
            TextMeshProUGUI promptText = promptInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (promptText != null)
            {
                promptText.text = interactable.GetInteractionPrompt();
            }
            activePrompts.Add(interactable, promptInstance);
            UpdatePromptPosition(promptInstance, worldPosition);
        }

        promptInstance.SetActive(true);
        return promptInstance;
    }

    /// <summary>
    /// Hides and optionally cleans up an interaction prompt.
    /// </summary>
    /// <param name="interactable">The interactable whose prompt to hide.</param>
    public void HidePrompt(IInteractable interactable)
    {
        if (interactable != null && activePrompts.TryGetValue(interactable, out GameObject promptInstance))
        {
            promptInstance.SetActive(false); // Or Destroy(promptInstance) if not pooling
            activePrompts.Remove(interactable); // Remove from dictionary
            // Clear the reference on the IInteractable as well
            interactable.CurrentWorldSpacePrompt = null;
        }
    }

    /// <summary>
    /// Updates the text of an existing prompt.
    /// </summary>
    /// <param name="interactable">The interactable associated with the prompt.</param>
    /// <param name="newText">The new text to display.</param>
    public void UpdatePromptText(IInteractable interactable, string newText)
    {
        if (activePrompts.TryGetValue(interactable, out GameObject promptInstance))
        {
            TextMeshProUGUI promptText = promptInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (promptText != null && promptText.text != newText) // Only update if text has changed
            {
                promptText.text = newText;
            }
        }
    }

    /// <summary>
    /// Updates the screen position of a prompt based on a world position.
    /// </summary>
    private void UpdatePromptPosition(GameObject promptInstance, Vector3 worldPosition)
    {
        // This is the crucial part for world-space UI
        if (Camera.main != null)
        {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 1.5f); // Adjust Y offset as needed
            promptInstance.transform.position = screenPosition;
            // You might need to adjust for canvas scaling and pivot here
        }
    }

    public bool IsPromptVisible()
    {
        // Simple check if any prompt is currently active
        foreach (var entry in activePrompts)
        {
            if (entry.Value.activeSelf)
            {
                return true;
            }
        }
        return false;
    }
}
