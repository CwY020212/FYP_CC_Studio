using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDialogueInteraction", menuName = "Interaction Definitions/Dialogue Interaction")]
public class DialogueInteractionDefinition : InteractionDefinition
{
    [Header("Dialogue Specific Settings")]
    [Tooltip("The lines of dialogue for this interaction.")]
    [TextArea(3, 10)] // Makes the string field a multi-line text area in the inspector
    public List<string> dialogueLines;

    // Optional: Add speaker names if you want different characters speaking
    public List<string> speakerNames;

    // Optional: Reference to a DialogueManager or similar script to handle the actual display
    // public DialogueManager dialogueManagerPrefab; // Or a reference to the scene's DialogueManager

    public override InteractionType GetInteractionType()
    {
        return InteractionType.Dialogue;
    }
}
