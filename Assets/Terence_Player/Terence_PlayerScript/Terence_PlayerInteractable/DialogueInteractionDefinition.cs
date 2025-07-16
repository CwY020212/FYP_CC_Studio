using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDialogueInteraction", menuName = "Interaction Definitions/Dialogue Interaction")]
public class DialogueInteractionDefinition : InteractionDefinition
{
    [Header("Dialogue Specific Settings")]
    [Tooltip("The lines of dialogue for this interaction.")]
    [TextArea(3, 10)]
    public List<string> dialogueLines;

    public List<string> speakerNames;

    [Header("Choices (Optional)")]
    [Tooltip("If the last line of dialogue offers choices.")]
    public bool offerChoices = false;
    public List<string> choiceTexts; // e.g., "Accept Quest", "Decline"
    // You might also add references to subsequent DialogueInteractionDefinitions
    // or events/actions tied to each choice. For a simple quest, just a flag.

    public override InteractionType GetInteractionType()
    {
        return InteractionType.Dialogue;
    }
}
