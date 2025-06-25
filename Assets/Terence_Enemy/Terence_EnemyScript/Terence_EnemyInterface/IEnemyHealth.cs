using UnityEngine;

public interface IEnemyHealth
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    int Defense { get; } // Or a method like GetDamageReduction(DamageType type)

    float HealthPercentage { get; }

    // Events for external subscribers
    event System.Action<int, int> OnHealthChange;
    event System.Action OnDeath;

    void TakeDamage(int damageAmount);
    void Heal(int amount);
}
