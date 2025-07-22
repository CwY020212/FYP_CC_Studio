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

    [Tooltip("The text for each choice. Make sure the count matches 'Next Dialogue Definitions'.")]
    public List<string> choiceTexts;

    [Tooltip("The DialogueInteractionDefinition to play after each choice. Must match 'Choice Texts' count.")]
    public List<DialogueInteractionDefinition> nextDialogueDefinitions; //This links choices to subsequent dialogues

    [Tooltip("Set to true if selecting a choice from this dialogue should end the entire dialogue sequence.")]
    public bool endsDialogueAfterChoice = false; //To indicate if a choice immediately ends the dialogue (e.g., declining quest)

    [Header("Quest Integration")]
    [Tooltip("If this dialogue definition, when played, should cause the linked quest to be accepted.")]
    public bool acceptsQuest = false;

    public override InteractionType GetInteractionType()
    {
        return InteractionType.Dialogue;
    }
}
