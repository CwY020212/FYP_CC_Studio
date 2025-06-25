using UnityEngine;

public class EnemyGuardHealth : MonoBehaviour, IEnemyHealth
{
    [Header("Health Attributes")]
    [SerializeField] private int maxHealth = 120;
    [SerializeField] private int currentHealth;
    [SerializeField] private int defense = 15; // Flat damage reduction

    // Interface implementations
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int Defense => defense;
    public float HealthPercentage => currentHealth / maxHealth;

    // Interface events
    public event System.Action<int, int> OnHealthChange;
    public event System.Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        // Apply defense
        damageAmount -= defense;
        if (damageAmount < 0) damageAmount = 0;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChange?.Invoke(currentHealth, maxHealth); // Invoke the event

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChange?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died!");
        OnDeath?.Invoke(); // Invoke the death event

        // Disable health component to prevent further damage
        this.enabled = false;
        // In a real game, trigger death animation, drop loot, etc.
        Destroy(gameObject, 3f); // Destroy after 3 seconds
        GetComponent<Collider>().enabled = false;
    }
}
