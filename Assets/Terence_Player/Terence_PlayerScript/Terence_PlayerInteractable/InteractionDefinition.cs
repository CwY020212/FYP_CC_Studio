using UnityEngine;

public abstract class InteractionDefinition : ScriptableObject
{
    [Header("General Interaction Settings")]
    [Tooltip("The prompt text displayed to the player (e.g., 'Press [E] to Open').")]
    public string interactionPromptText = "Press [E] to Interact";

    [Tooltip("The name of the player's Animator Trigger that initiates this interaction (e.g., 'Interact_OpenChest', 'EnterClimb', 'Talk').")]
    public string playerAnimationTrigger;

    [Tooltip("If this interaction opens a dedicated UI panel (like a dialogue screen or inventory). Assign a prefab of a UI Canvas.")]
    public GameObject dedicatedUIPrefab;

    // All subclasses MUST implement this to specify their type.
    public abstract InteractionType GetInteractionType();
}
