using UnityEngine;

[CreateAssetMenu(fileName = "Terence_PlayerWeaponSO", menuName = "Terence_PlayerWeaponItem/Terence_PlayerWeaponData")]
public class PlayerWeaponSO : ScriptableObject
{
    [Header("Weapon Model")]
    public GameObject weaponPrefab;

    [Header("Weapon Details")]
    public string weaponName;
    public Sprite weaponIcon;
    public int weaponIndex;
    public string weaponDescription;

    [Header("Weapon Settings")]
    public PlayerWeaponElement weaponElement; // Assuming PlayerWeaponElement is an enum
    public float weaponLightDmg;
    public float weaponHeavyDmg;
    public float weaponUltimateDmg;
    public float weaponSkillDmg;

    // Added Stamina Costs for Attacks
    [Header("Stamina Costs")]
    public float lightAttackStaminaCost = 10f;
    public float heavyAttackStaminaCost = 25f;
    public float skillAttackStaminaCost = 30f;
    public float ultimateAttackStaminaCost = 50f;

    // Added Cooldowns for Skill and Ultimate Attacks
    [Header("Cooldowns")]
    public float ultimateAttackCooldown = 60f;
    public float skillAttackCooldown = 30f;

    [Header("Weapon Animations")]
    public AnimationClip lightAttack;
    public AnimationClip heavyAttack;
    public AnimationClip ultimateAttack;
    public AnimationClip skillAttack;

    [Header("Weapon Spawned Position")]
    public Vector3 weaponInGameSpawnPoint;
    public Vector3 weaponHoldingSpawnPoint;
}
