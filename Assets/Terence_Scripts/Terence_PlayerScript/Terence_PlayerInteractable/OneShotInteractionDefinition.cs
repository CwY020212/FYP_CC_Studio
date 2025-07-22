using UnityEngine;

[CreateAssetMenu(fileName = "NewOneShotInteraction", menuName = "Interaction Definitions/One-Shot Interaction")]
public class OneShotInteractionDefinition : InteractionDefinition
{
    public override InteractionType GetInteractionType()
    {
        return InteractionType.OneShot;
    }

    // Add any properties specific to One-Shot interactions here if needed.
    // For example, a sound effect to play on completion, or a particle effect.
    // [Tooltip("Sound effect to play when the interaction completes.")]
    // public AudioClip completionSound;
}