using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private PlayerWeaponSO defaultBrawlerWeapon; // Default for unarmed
    [SerializeField] private PlayerWeaponSO defaultOffHandWeapon; // Optional: default for off-hand if separate

    [Header("Weapon Holding Slots")]
    public Transform rightHandWeaponSlot; // Primary hand
    public Transform leftHandWeaponSlot;  // Off-hand

    public PlayerWeaponSO CurrentRightHandWeapon { get; private set; } // Weapon in right hand
    public PlayerWeaponSO CurrentLeftHandWeapon { get; private set; }  // Weapon in left hand

    private GameObject _currentEquippedRightHandModel;
    private GameObject _currentEquippedLeftHandModel;

    private void Awake()
    {
        // Initialize default weapons for both hands if they are assigned
        if (defaultBrawlerWeapon != null)
        {
            SetCurrentWeapon(defaultBrawlerWeapon, WeaponHand.RightHand, false); // Right hand default (e.g., fists)
        }
        if (defaultOffHandWeapon != null)
        {
            SetCurrentWeapon(defaultOffHandWeapon, WeaponHand.LeftHand, false); // Left hand default (e.g., empty or shield)
        }
        else if (defaultBrawlerWeapon != null) // If no specific off-hand default, use brawler for both
        {
            SetCurrentWeapon(defaultBrawlerWeapon, WeaponHand.LeftHand, false);
        }
    }

    /// <summary>
    /// Sets a new weapon for a specific hand.
    /// </summary>
    /// <param name="newWeapon">The PlayerWeaponSO for the new weapon.</param>
    /// <param name="hand">Which hand to equip the weapon in.</param>
    /// <param name="equipModel">Whether to instantiate and equip the 3D model.</param>
    public void SetCurrentWeapon(PlayerWeaponSO newWeapon, WeaponHand hand, bool equipModel = true)
    {
        if (hand == WeaponHand.RightHand)
        {
            CurrentRightHandWeapon = newWeapon;
            Debug.Log($"Player now equipped with: {newWeapon.weaponName} in Right Hand.");
            if (equipModel)
            {
                EquipWeaponModel(newWeapon, hand);
            }
        }
        else if (hand == WeaponHand.LeftHand)
        {
            CurrentLeftHandWeapon = newWeapon;
            Debug.Log($"Player now equipped with: {newWeapon.weaponName} in Left Hand.");
            if (equipModel)
            {
                EquipWeaponModel(newWeapon, hand);
            }
        }
    }

    private void EquipWeaponModel(PlayerWeaponSO weaponSO, WeaponHand hand)
    {
        // Determine which slot and current model to manage based on hand
        Transform targetSlot = (hand == WeaponHand.RightHand) ? rightHandWeaponSlot : leftHandWeaponSlot;
        GameObject currentModel = (hand == WeaponHand.RightHand) ? _currentEquippedRightHandModel : _currentEquippedLeftHandModel;

        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        if (weaponSO != null && weaponSO.weaponPrefab != null)
        {
            GameObject newModel = Instantiate(weaponSO.weaponPrefab, targetSlot);
            newModel.transform.localPosition = weaponSO.weaponHoldingSpawnPoint;
            newModel.transform.localRotation = Quaternion.identity;
            Debug.Log($"Spawned {weaponSO.weaponName} model in {hand}.");

            // Update the correct equipped model reference
            if (hand == WeaponHand.RightHand)
            {
                _currentEquippedRightHandModel = newModel;
            }
            else
            {
                _currentEquippedLeftHandModel = newModel;
            }
        }
        else // If weaponSO is null or weaponPrefab is null, ensure no model is held
        {
            if (hand == WeaponHand.RightHand)
            {
                _currentEquippedRightHandModel = null;
            }
            else
            {
                _currentEquippedLeftHandModel = null;
            }
            Debug.LogWarning($"Attempted to equip a null weapon or prefab in {hand}. Slot cleared.");
        }
    }

    /// <summary>
    /// Unequips the weapon in a specific hand.
    /// </summary>
    /// <param name="hand">The hand from which to unequip the weapon.</param>
    public void UnequipWeapon(WeaponHand hand)
    {
        if (hand == WeaponHand.RightHand)
        {
            if (_currentEquippedRightHandModel != null)
            {
                Destroy(_currentEquippedRightHandModel);
                _currentEquippedRightHandModel = null;
            }
            CurrentRightHandWeapon = defaultBrawlerWeapon;
            Debug.Log($"Right hand weapon unequipped, reverting to {defaultBrawlerWeapon.weaponName}.");
        }
        else if (hand == WeaponHand.LeftHand)
        {
            if (_currentEquippedLeftHandModel != null)
            {
                Destroy(_currentEquippedLeftHandModel);
                _currentEquippedLeftHandModel = null;
            }
            CurrentLeftHandWeapon = defaultOffHandWeapon ?? defaultBrawlerWeapon; // Revert to off-hand default or brawler
            Debug.Log($"Left hand weapon unequipped, reverting to {CurrentLeftHandWeapon.weaponName}.");
        }
    }

    /// <summary>
    /// Enum to specify which hand a weapon belongs to.
    /// </summary>
    public enum WeaponHand
    {
        RightHand,
        LeftHand
    }
}
