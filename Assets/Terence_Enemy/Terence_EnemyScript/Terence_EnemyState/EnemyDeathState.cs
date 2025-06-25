using UnityEngine;
using UnityEngine.AI;

public class EnemyDeathState : EnemyBaseState
{
    public EnemyDeathState(EnemyAIStateMachine stateMachine, NavMeshAgent agent, Transform playerTransform, Animator animator, IEnemyHealth enemyHealth)
        : base(stateMachine, agent, playerTransform, animator, enemyHealth) { }

    public override void EnterState()
    {
        Debug.Log("Entering Death State");
        if (animator != null) animator.SetTrigger("Die");
        agent.isStopped = true;
        // Disable relevant components/colliders upon death
        stateMachine.GetComponent<Collider>().enabled = false;
        stateMachine.GetComponent<Rigidbody>().isKinematic = true; // Stop physics interactions
        stateMachine.enabled = false; // Disable the AI controller itself

        // In a real game, you might:
        // - Play death sound
        // - Instantiate loot
        // - Trigger ragdoll
        // - Disable/Destroy this GameObject after a delay (handled by EnemyHealth for now)
    }

    public override void UpdateState()
    {
        // Nothing to do in Update once dead
    }

    public override void ExitState()
    {
        // This state is usually terminal, so ExitState might not be called in practice
    }
}
