using UnityEngine;
using UnityEngine.AI;

public class EnemyDetectionState : EnemyBaseState
{
    private float detectionTimer;

    public EnemyDetectionState(EnemyAIStateMachine stateMachine, NavMeshAgent agent, Transform playerTransform, Animator animator, IEnemyHealth enemyHealth)
        : base(stateMachine, agent, playerTransform, animator, enemyHealth) { }

    public override void EnterState()
    {
        Debug.Log("Entering Detection State");
        detectionTimer = 0f;
        if (animator != null) animator.SetBool("IsChasing", false); // Maybe play an alert anim
        if (animator != null) animator.SetBool("IsAttacking", false);
        if (playerTransform != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(playerTransform.position); // Move slowly towards player
        }
    }

    public override void UpdateState()
    {
        if (playerTransform == null) { stateMachine.ChangeState(stateMachine.patrolState); return; }

        FaceTarget(playerTransform.position); // Always face the detected player

        detectionTimer += Time.deltaTime;
        if (detectionTimer >= stateMachine.DetectionConfirmTime)
        {
            stateMachine.ChangeState(stateMachine.aggressiveState); // Confirmed threat
        }
        else if (Vector3.Distance(stateMachine.transform.position, playerTransform.position) > stateMachine.DetectionRange + 2f)
        {
            stateMachine.ChangeState(stateMachine.patrolState); // Player left range before threat confirmed
        }

        // Keep moving towards player in detection, or stop if very close to observe
        if (agent.enabled && agent.isOnNavMesh && Vector3.Distance(stateMachine.transform.position, playerTransform.position) > agent.stoppingDistance)
        {
            agent.SetDestination(playerTransform.position);
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Detection State");
    }

    public override void OnDamageTaken()
    {
        stateMachine.ChangeState(stateMachine.aggressiveState); // Force aggro on hit
    }
}