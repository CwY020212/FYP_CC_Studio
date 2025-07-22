using UnityEngine;

public class PickableWeapon : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerWeaponSO weaponToEquip;
    [SerializeField] private string interactionPromptText = "Pick up weapon"; // Text for the UI prompt

    // This is where InteractionPromptManager would store the reference to the spawned UI element
    public GameObject CurrentWorldSpacePrompt { get; set; }

    public bool CanInteract(PlayerStateMachine player)
    {
        return weaponToEquip != null;
    }

    public void Interact(PlayerStateMachine player)
    {
        if (weaponToEquip == null)
        {
            Debug.LogWarning("PickableWeapon has no weaponToEquip assigned!", this);
            return;
        }

        // MODIFIED: Now specifying the hand when setting the weapon.
        // Assuming pickable weapons go into the right hand by default.
        player.weaponManager.SetCurrentWeapon(weaponToEquip, PlayerWeaponManager.WeaponHand.RightHand, true);

        // Optionally, destroy the pickable weapon object after it's picked up
        Destroy(gameObject);

        // Hide the prompt after interaction
        InteractionPromptManager.Instance?.HidePrompt(this);
    }

    public string GetInteractionPrompt()
    {
        return interactionPromptText;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = Color.cyan;
    }
}