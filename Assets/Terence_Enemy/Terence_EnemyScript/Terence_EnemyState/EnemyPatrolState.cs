using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : EnemyBaseState
{
    private Transform[] patrolWaypoints;
    private float waypointTolerance;
    private int currentWaypointIndex = 0;

    private float restTimer; // Timer for the rest period
    private bool isResting; // Flag to indicate if the enemy is resting

    // Add a new parameter for rest duration in the constructor
    private float restDuration;

    public EnemyPatrolState(EnemyAIStateMachine stateMachine, NavMeshAgent agent, Transform playerTransform, Animator animator, IEnemyHealth enemyHealth, Transform[] waypoints, float tolerance, float restDurationAtWaypoint)
        : base(stateMachine, agent, playerTransform, animator, enemyHealth)
    {
        patrolWaypoints = waypoints;
        waypointTolerance = tolerance;
        restDuration = restDurationAtWaypoint; // Store the rest duration
    }

    public override void EnterState()
    {
        Debug.Log("Entering Patrol State");
        agent.speed = stateMachine.PatrolSpeed;
        if (animator != null) animator.SetBool("IsChasing", false);
        if (animator != null) animator.SetBool("IsAttacking", false);

        isResting = false; // Start by not resting
        SetNewPatrolDestination(); // Immediately set the first destination
    }

    public override void UpdateState()
    {
        // Check for player detection (always high priority)
        if (playerTransform != null && Vector3.Distance(stateMachine.transform.position, playerTransform.position) <= stateMachine.DetectionRange)
        {
            Vector3 directionToPlayer = (playerTransform.position - stateMachine.transform.position).normalized;
            if (Vector3.Dot(stateMachine.transform.forward, directionToPlayer) > Mathf.Cos(Mathf.Deg2Rad * (stateMachine.ViewConeAngle / 2)))
            {
                stateMachine.ChangeState(stateMachine.detectionState); // Transition!
                return;
            }
        }

        // --- Patrol Logic with Rest Period ---
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;

        if (isResting)
        {
            // If resting, count down the timer
            restTimer -= Time.deltaTime;
            if (restTimer <= 0)
            {
                isResting = false; // Rest period over
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length; // Move to next waypoint
                SetNewPatrolDestination(); // Set new destination
            }
        }
        else // Not resting, currently patrolling/moving
        {
            // If agent is close to destination and not currently processing a path
            if (agent.enabled && agent.isOnNavMesh && agent.remainingDistance < waypointTolerance && !agent.pathPending)
            {
                // Reached waypoint, start resting
                isResting = true;
                restTimer = restDuration; // Initialize rest timer
                agent.isStopped = true; // Stop the agent
                if (animator != null) animator.SetBool("IsWalking", false); // Stop walk animation
                if (animator != null) animator.SetBool("IsIdle", true);    // Play idle animation
                Debug.Log($"Reached Waypoint {currentWaypointIndex}. Resting for {restDuration} seconds.");
            }
            else // Still moving towards a waypoint
            {
                agent.isStopped = false; // Ensure agent is moving
                if (animator != null) animator.SetBool("IsWalking", true); // Play walk animation
                if (animator != null) animator.SetBool("IsIdle", false);    // Stop idle animation
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Patrol State");
        if (agent.enabled && agent.isOnNavMesh) agent.ResetPath();
        if (animator != null) animator.SetBool("IsWalking", false); // Ensure walk/idle is off
        if (animator != null) animator.SetBool("IsIdle", false);
    }

    private void SetNewPatrolDestination()
    {
        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
                agent.isStopped = false; // Ensure agent starts moving
                if (animator != null) animator.SetBool("IsWalking", true); // Start walk animation
                if (animator != null) animator.SetBool("IsIdle", false);    // Stop idle animation
                Debug.Log($"Setting destination to Waypoint {currentWaypointIndex}: {patrolWaypoints[currentWaypointIndex].position}");
            }
        }
    }

    public override void OnDamageTaken()
    {
        stateMachine.ChangeState(stateMachine.aggressiveState); // Force aggro on hit
    }
}