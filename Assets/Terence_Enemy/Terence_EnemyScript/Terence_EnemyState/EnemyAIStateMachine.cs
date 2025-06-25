using UnityEngine;
using UnityEngine.AI;

public class EnemyAIStateMachine : MonoBehaviour
{
    [Header("AI Core References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTransform; // Assign in Inspector or find by Tag
    [SerializeField] private Animator animator;
    private IEnemyHealth enemyHealth; // Using the interface!

    [Header("State Parameters - Pahangan Guard")]
    public float PatrolSpeed = 2.5f;
    public float ChaseSpeed = 4f;
    public float DetectionRange = 15f;
    public float ViewConeAngle = 120f; // Half angle for dot product calculation is ViewConeAngle / 2
    public float AggroRange = 20f;
    public float ResetRange = 30f;
    public float DetectionConfirmTime = 1.5f;

    // Combat parameters (moved from AggressiveState for centralized control)
    public float AttackRange = 2f;
    public float BasicAttackCooldown = 1.5f;

    // Critical HP for specific state transitions
    public float CriticalHPThreshold = 0.4f; // 40%

    // State instances
    public EnemyBaseState currentState;
    public EnemyPatrolState patrolState;
    public EnemyDetectionState detectionState;
    public EnemyAggressiveState aggressiveState;
    public EnemyGetHitState getHitState;
    public EnemyDeathState deathState;

    [Header("Patrol Settings")]
    public Transform[] patrolWaypoints; // Assign these in the Inspector
    public float waypointTolerance = 0.5f; // How close to a waypoint before it's considered "reached"
    public float patrolRestDuration = 2.0f;

    void Awake()
    {
        // Get component references
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        enemyHealth = GetComponent<IEnemyHealth>(); // Get the interface!

        // Initialize all states
        patrolState = new EnemyPatrolState(this, agent, playerTransform, animator, enemyHealth, patrolWaypoints, waypointTolerance, patrolRestDuration);
        detectionState = new EnemyDetectionState(this, agent, playerTransform, animator, enemyHealth);
        aggressiveState = new EnemyAggressiveState(this, agent, playerTransform, animator, enemyHealth);
        getHitState = new EnemyGetHitState(this, agent, playerTransform, animator, enemyHealth);
        deathState = new EnemyDeathState(this, agent, playerTransform, animator, enemyHealth);

        // Subscribe to health events
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += HandleDeath;
            // Subscribe to damage taken to potentially trigger GetHit state
            enemyHealth.OnHealthChange += (current, max) =>
            {
                if (currentState != deathState) // Only if not already dead
                {
                    currentState?.OnDamageTaken(); // Notify current state of damage taken
                }
            };
        }
    }

    void Start()
    {
        // Find player if not assigned (good for quick setup)
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player GameObject not found. Please tag your player as 'Player'.");
            }
        }

        ChangeState(patrolState); // Start in Patrol state
    }

    void Update()
    {
        currentState?.UpdateState(); // Call Update on the current active state
    }

    public void ChangeState(EnemyBaseState newState)
    {
        if (newState == null)
        {
            Debug.LogError("Attempting to change to a null state!");
            return;
        }

        if (currentState == newState) return; // Already in this state

        currentState?.ExitState(); // Call Exit on the current state (if any)
        currentState = newState;   // Set the new state
        currentState.EnterState(); // Call Enter on the new state
    }

    // Handle death event from EnemyHealth
    private void HandleDeath()
    {
        ChangeState(deathState);
    }

    // Example for Animation Event to apply damage (called from Animator)
    public void OnBasicAttackHit()
    {
        if (playerTransform == null) return;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= AttackRange + 0.5f) // Give a small buffer for hit registration
        {
            // You'll need an IPlayerHealth interface or PlayerHealth script on your player
            // For example:
            HealthSystem playerHealth = playerTransform.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(enemyHealth.Defense); // Placeholder: Use enemy's AttackDamage attribute directly
            }
            Debug.Log($"<color=red>Enemy attacked Player for {enemyHealth.Defense} damage!</color>"); // Assuming Defense attribute holds attack value for now
        }
    }

    // Visualization in Editor (same as before)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AggroRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, ResetRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        if (patrolWaypoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolWaypoints.Length; i++)
            {
                Gizmos.DrawSphere(patrolWaypoints[i].position, 0.5f);
                if (i < patrolWaypoints.Length - 1)
                {
                    Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[i + 1].position);
                }
                else if (patrolWaypoints.Length > 1) // Loop back to first waypoint
                {
                    Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[0].position);
                }
            }
        }
    }
    public void OnBasicAttackAnimationEnd()
    {
        // This function will be called by the animation event when the attack animation finishes.
        Debug.Log("Basic Attack Animation Finished!");
        // Transition back to AggressiveState or another appropriate state
        ChangeState(aggressiveState); // Or perhaps MovementState/ChaseState if you had one
                                      // The key is to get out of the "stuck in attack" feeling
    }
}
