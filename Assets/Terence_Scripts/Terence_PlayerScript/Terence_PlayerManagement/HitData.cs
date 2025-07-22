using UnityEngine;

public struct HitData
{
    public int damageAmount;
    public Vector3 hitPoint; // Point of impact on the player
    public Vector3 attackerPosition; // World position of the attacker
    public float knockbackForce;
    public float hitStunDuration;
    public bool activatesFullRagdoll;
    public Vector3 initialRagdollImpulse;
}