using UnityEngine;
using UnityEngine.AI;

public class EnemyGetHitState : EnemyBaseState
{
    public EnemyGetHitState(EnemyAIStateMachine stateMachine, NavMeshAgent agent, Transform playerTransform, Animator animator, IEnemyHealth enemyHealth)
        : base(stateMachine, agent, playerTransform, animator, enemyHealth) { }

    public override void EnterState()
    {
        Debug.Log("Entering GetHit State");
        if (animator != null) animator.SetTrigger("Hit");
        agent.isStopped = true; // Stop movement during hit reaction
        // You might disable hitboxes, etc.

        // After a short hit reaction, return to the aggressive state
        // In a real game, this might be based on animation events or a more complex stagger system.
        stateMachine.StartCoroutine(ReturnFromGetHitAfterDelay(0.5f)); // Use stateMachine to start coroutine
    }

    public override void  UpdateState()
    {
        // No specific update logic needed for a short reaction state
    }

    public override void ExitState()
    {
        Debug.Log("Exiting GetHit State");
        agent.isStopped = false; // Allow movement again
    }

    // Coroutine to handle the short duration of the GetHit state
    private System.Collections.IEnumerator ReturnFromGetHitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Decide next state based on health and enemy type (as per design doc)
        if (enemyHealth.HealthPercentage <= stateMachine.CriticalHPThreshold)
        {
            // Example for "Pahangan Guard" entering Guard State
            // You'd need specific logic here per enemy type or a more generic system
            // if (stateMachine.EnemyType == EnemyType.Guard)
            // {
            //     stateMachine.ChangeState(stateMachine.guardState);
            // } else if (stateMachine.EnemyType == EnemyType.Attack)
            // {
            //     stateMachine.ChangeState(stateMachine.fleeState);
            // }
            // For now, a simple fallback:
            stateMachine.ChangeState(stateMachine.aggressiveState); // Default to aggressive if not specialized
        }
        else
        {
            stateMachine.ChangeState(stateMachine.aggressiveState);
        }
    }
}
