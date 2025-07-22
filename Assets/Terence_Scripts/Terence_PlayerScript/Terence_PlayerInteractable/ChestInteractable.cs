using UnityEngine;
using TMPro; 

public class ChestInteractable : MonoBehaviour, IInteractable
{
    [Header("Chest Specific Settings")]
    [Tooltip("Assign the OneShotInteractionDefinition Scriptable Object here.")]
    [SerializeField] private OneShotInteractionDefinition interactionDefinition; // DRAG YOUR SO HERE IN INSPECTOR

    [Tooltip("The Animator component on the chest itself.")]
    [SerializeField] private Animator chestAnimator;

    [Tooltip("The name of the Animator Trigger on the chest to open it (e.g., 'Open').")]
    [SerializeField] private string chestOpenAnimationTrigger = "Open";

    [Tooltip("The text to display on the interaction prompt (e.g., 'Open Chest').")]
    [SerializeField] private string interactionPromptText = "Open Chest"; // ADDED: Field for the prompt text

    private bool isOpen = false;

    // From IInteractable interface
    public GameObject CurrentWorldSpacePrompt { get; set; }

    private void Start()
    {
        if (chestAnimator == null)
        {
            chestAnimator = GetComponent<Animator>();
            if (chestAnimator == null)
            {
                Debug.LogWarning("Chest Animator not assigned or found on " + gameObject.name, this); // Added 'this' for context
            }
        }

        if (interactionDefinition == null)
        {
            Debug.LogError("ChestInteractable is missing its OneShotInteractionDefinition! Assign it in the Inspector.", this);
        }
    }

    public void Interact(PlayerStateMachine player)
    {
        if (isOpen)
        {
            Debug.Log("Chest is already open.");
            return;
        }

        Debug.Log("Performing Chest Interaction: Opening Chest and giving loot.");
        isOpen = true; // Mark as open immediately

        // Hide the prompt once interacted with, as it's now open
        InteractionPromptManager.Instance?.HidePrompt(this); // Assuming 'this' as the IInteractable reference

        // Trigger the chest's own animation
        if (chestAnimator != null && !string.IsNullOrEmpty(chestOpenAnimationTrigger))
        {
            chestAnimator.SetTrigger(chestOpenAnimationTrigger);
            Debug.Log($"Chest animation '{chestOpenAnimationTrigger}' triggered.");
        }

        GiveLoot(); // Your game-specific logic for loot
    }

    // From IInteractable: Checks if interaction is currently possible
    public bool CanInteract(PlayerStateMachine player)
    {
        // Player can only interact if the chest is not already open
        return !isOpen;
    }

    // From IInteractable: Returns the text for the interaction prompt
    public string GetInteractionPrompt()
    {
        return interactionPromptText;
    }

    // ADDED: If your IInteractable interface includes GetInteractionDefinition(), implement it:
    // public OneShotInteractionDefinition GetInteractionDefinition()
    // {
    //     return interactionDefinition;
    // }

    // Placeholder for your game's loot mechanics
    private void GiveLoot()
    {
        Debug.Log("Looting items from chest! (Implement your actual loot logic here)");
        // Example: InventoryManager.Instance.AddGold(100);
        // Example: LootTable.GenerateLoot(this.transform.position);
    }
}