using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class StaminaSystem : MonoBehaviour
{
    // Public reference to the PlayerStateMachine, will be assigned in Start()
    public PlayerStateMachine context;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float sprintStaminaDrainRate = 15f; // Changed to drain rate per second
    public float dodgeStaminaCost = 25f;       // Single cost for dodge
    public float staminaReplenishRate = 10f;  // Rate per second
    public float staminaReplenishDelay = 1.5f; // Delay before replenishment starts

    [Header("Stamina UI")]
    public Slider StaminaBar;

    public static StaminaSystem instance; // Singleton pattern for easy access
    private Coroutine activeReplenishCoroutine;
    private bool isStaminaBeingUsed = false; // Flag to prevent replenishment

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentStamina = maxStamina;
        // Find PlayerStateMachine on the same GameObject or through another method if needed
        context = GetComponent<PlayerStateMachine>();
        if (StaminaBar != null)
        {
            StaminaBar.maxValue = maxStamina;
            StaminaBar.value = currentStamina;
        }
    }

    void Update()
    {
        // Update UI in Update if it needs to be very responsive
        if (StaminaBar != null)
        {
            StaminaBar.value = currentStamina;
        }
    }

    void LateUpdate()
    {
        // Determine if stamina is currently being used for continuous actions like sprinting
        // This should observe the PlayerStateMachine's *current state* rather than direct input
        // Since PlayerStateMachine manages transitions, it knows if it's in a running state.
        isStaminaBeingUsed = (context.currentState == context.runState);

        HandleStaminaReplenishmentLogic();
    }

    // --- Public Methods for Other Systems to Use ---

    public bool CanSpendStamina(float amount)
    {
        return currentStamina >= amount;
    }

    public void SpendStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(0f, currentStamina);
        StopStaminaReplenishment(); // Stop replenishment immediately when stamina is spent
        // You might want to start a short delay before replenishment here too
        RestartStaminaReplenishmentDelayed();
    }

    // Method to continuously drain stamina (e.g., for sprinting)
    public void DrainStaminaOverTime(float rate)
    {
        currentStamina -= rate * Time.deltaTime;
        currentStamina = Mathf.Max(0f, currentStamina);
        StopStaminaReplenishment(); // Stop replenishment while draining
        // If currentStamina reaches zero due to draining, notify the state machine
        if (currentStamina <= 0.01f && context.currentState == context.runState) // A small epsilon for float comparison
        {
            // The PlayerStateMachine should handle this transition,
            // e.g., by checking if stamina is too low in its RunState Update.
            // For now, we can force a transition for demonstration.
            Debug.Log("Stamina depleted, stopping run.");
            context.SwitchState(context.idleState); // Force back to idle
            // Also stop the run input in PlayerInputHandler if it's a toggle
            // (You'd need to add a method to PlayerInputHandler to clear run input)
        }
    }

    // --- Internal Replenishment Logic ---

    void HandleStaminaReplenishmentLogic()
    {
        // If not actively using stamina (e.g., not sprinting) and below max
        if (!isStaminaBeingUsed && currentStamina < maxStamina)
        {
            if (activeReplenishCoroutine == null)
            {
                activeReplenishCoroutine = StartCoroutine(ReplenishStaminaOverTime());
            }
        }
        // If stamina is at max or being used, stop replenishment
        else if (currentStamina >= maxStamina || isStaminaBeingUsed)
        {
            StopStaminaReplenishment();
        }
    }

    IEnumerator ReplenishStaminaOverTime()
    {
        yield return new WaitForSeconds(staminaReplenishDelay); // Wait before starting replenishment

        while (currentStamina < maxStamina && !isStaminaBeingUsed)
        {
            currentStamina += staminaReplenishRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
            yield return null;
        }
        activeReplenishCoroutine = null; // Clear coroutine reference when done
    }

    void StopStaminaReplenishment()
    {
        if (activeReplenishCoroutine != null)
        {
            StopCoroutine(activeReplenishCoroutine);
            activeReplenishCoroutine = null;
        }
    }

    void RestartStaminaReplenishmentDelayed()
    {
        StopStaminaReplenishment();
        activeReplenishCoroutine = StartCoroutine(ReplenishStaminaOverTime());
    }
}
