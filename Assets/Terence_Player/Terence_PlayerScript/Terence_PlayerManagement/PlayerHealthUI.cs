using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public HealthSystem playerHealthSystem; // Drag your player's HealthSystem here in the Inspector
    public Slider healthBarSlider; // Drag your UI Slider here
    public TextMeshProUGUI healthText; // Drag your UI TextMeshProUGUI here

    void OnEnable()
    {
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged += UpdateHealthUI;
            playerHealthSystem.OnDied += OnPlayerDied;
        }
    }

    void OnDisable()
    {
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged -= UpdateHealthUI;
            playerHealthSystem.OnDied -= OnPlayerDied;
        }
    }

    void Start()
    {
        // Initial UI update in case the event was missed
        if (playerHealthSystem != null)
        {
            UpdateHealthUI(playerHealthSystem.CurrentHealth, playerHealthSystem.MaxHealth);
        }
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}/{maxHealth}";
        }
    }

    private void OnPlayerDied()
    {
        Debug.Log("UI detected player death. Showing Game Over screen or similar.");
        // Example:
        // gameObject.SetActive(false); // Hide the health UI
        // GameManager.Instance.ShowGameOverScreen();
    }
}
