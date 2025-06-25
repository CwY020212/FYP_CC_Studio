using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("The maximum health this entity can have.")]
    [SerializeField] private int maxHealth = 100;

    [Tooltip("The current health of this entity.")]
    [SerializeField] private int currentHealth;

    // Public properties to access health values
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    // Public property to check if the entity is dead
    public bool IsDead { get; private set; } = false;

    // Events to notify other systems
    // Action<int currentHealth, int maxHealth>: Notifies when health changes
    public event Action<int, int> OnHealthChanged;

    // Action: Notifies when the entity dies
    public event Action OnDied;

    void Awake()
    {
        // Initialize current health to max health when the game starts
        InitializeHealth();
    }

    /// <summary>
    /// Initializes or resets the health of this entity to its maximum value.
    /// </summary>
    public void InitializeHealth()
    {
        currentHealth = maxHealth;
        IsDead = false;
        // Notify listeners that health has been initialized/reset
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Reduces the current health by a specified amount.
    /// Clamps health at 0 and triggers the OnDied event if health reaches zero.
    /// </summary>
    /// <param name="amount">The amount of damage to take.</param>
    public void TakeDamage(int amount)
    {
        if (IsDead)
        {
            Debug.Log($"{gameObject.name} is already dead and cannot take more damage.");
            return;
        }

        if (amount < 0)
        {
            Debug.LogWarning("Damage amount cannot be negative. Use Heal() for healing.");
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Ensure health doesn't go below 0

        Debug.Log($"{gameObject.name} took {amount} damage. Current Health: {currentHealth}");

        // Notify listeners about the health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0 && !IsDead)
        {
            Die();
        }
    }

    /// <summary>
    /// Increases the current health by a specified amount.
    /// Clamps health at maxHealth.
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
    public void Heal(int amount)
    {
        if (IsDead)
        {
            Debug.Log($"{gameObject.name} is dead and cannot be healed (or needs revival logic).");
            return;
        }

        if (amount < 0)
        {
            Debug.LogWarning("Heal amount cannot be negative. Use TakeDamage() for damage.");
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Ensure health doesn't exceed maxHealth

        Debug.Log($"{gameObject.name} healed for {amount}. Current Health: {currentHealth}");

        // Notify listeners about the health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Marks the entity as dead and triggers the OnDied event.
    /// This method is automatically called by TakeDamage() if health drops to 0.
    /// It can also be called externally for instant death scenarios.
    /// </summary>
    public void Die()
    {
        if (IsDead) return; // Prevent multiple death calls

        IsDead = true;
        Debug.Log($"{gameObject.name} has died!");

        // Notify listeners that the entity has died
        OnDied?.Invoke();

        // Optional: Disable or destroy the GameObject
        // For players, you might want to transition to a Death State in your state machine.
        // For enemies, you might play a death animation, disable AI, then destroy after a delay.
        // gameObject.SetActive(false); // Example: just deactivate the object
        // Destroy(gameObject, 5f); // Example: destroy after 5 seconds
    }

    // --- Example Usage / Debugging ---
    // You can add this for testing in the editor.
    [ContextMenu("Test Take 10 Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(10);
    }

    [ContextMenu("Test Heal 5 Health")]
    private void TestHeal()
    {
        Heal(5);
    }

    [ContextMenu("Test Instant Death")]
    private void TestInstantDeath()
    {
        Die();
    }
}
