using UnityEngine;

public interface IInteractable
{
    bool CanInteract(PlayerStateMachine player);
    void Interact(PlayerStateMachine player);
    GameObject CurrentWorldSpacePrompt { get; set; } // For managing the UI prompt
    string GetInteractionPrompt(); // To get text for the prompt
}
