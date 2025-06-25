using UnityEngine;
using UnityEngine.AI;

public class EnemyAggressiveState : EnemyBaseState
{
    private float lastBasicAttackTime;
    private bool isAttackingAnimationPlaying; // New flag

    public EnemyAggressiveState(EnemyAIStateMachine stateMachine, NavMeshAgent agent, Transform playerTransform, Animator animator, IEnemyHealth enemyHealth)
        : base(stateMachine, agent, playerTransform, animator, enemyHealth) { }

    public override void EnterState()
    {
        Debug.Log("Entering Aggressive State");
        agent.speed = stateMachine.ChaseSpeed;
        if (animator != null) animator.SetBool("IsChasing", true);
        if (animator != null) animator.SetBool("IsAttacking", false); // Ensure attack bool is false on entry
        lastBasicAttackTime = Time.time - stateMachine.BasicAttackCooldown; // Allow immediate attack
        isAttackingAnimationPlaying = false; // Reset attack flag
    }

    public override void UpdateState()
    {
        if (playerTransform == null) { stateMachine.ChangeState(stateMachine.patrolState); return; }

        float distanceToPlayer = Vector3.Distance(stateMachine.transform.position, playerTransform.position);

        // Global check: Reset if player too far
        if (distanceToPlayer > stateMachine.ResetRange)
        {
            stateMachine.ChangeState(stateMachine.patrolState);
            return;
        }

        // Only perform actions if not currently in an attack animation
        if (!isAttackingAnimationPlaying) // CHECK THIS FLAG
        {
            // Movement & Attack Logic
            if (distanceToPlayer > stateMachine.AttackRange)
            {
                // Chase player
                if (agent.enabled && agent.isOnNavMesh) agent.SetDestination(playerTransform.position);
                agent.isStopped = false;
                if (animator != null) animator.SetBool("IsChasing", true);
                if (animator != null) animator.SetBool("IsAttacking", false);
            }
            else
            {
                // Attack logic
                agent.isStopped = true;
                if (animator != null) animator.SetBool("IsChasing", false);
                FaceTarget(playerTransform.position);

                if (Time.time >= lastBasicAttackTime + stateMachine.BasicAttackCooldown)
                {
                    PerformBasicAttack();
                }
            }
        }
        // If isAttackingAnimationPlaying is true, we just wait for the animation event to finish.
        // TODO: Implement Heavy Attack logic here
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Aggressive State");
        if (animator != null) animator.SetBool("IsChasing", false);
        if (animator != null) animator.SetBool("IsAttacking", false);
        isAttackingAnimationPlaying = false; // Ensure this is reset
    }

    private void PerformBasicAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("BasicAttack");
            // Optionally, if you have an "IsAttacking" boolean that you want to hold for the duration of the animation:
            // animator.SetBool("IsAttacking", true); // You would need a transition out of IsAttacking when animation ends
            // For triggers, usually just the trigger is enough.
        }
        Debug.Log($"{stateMachine.gameObject.name} performs Basic Attack!");
        lastBasicAttackTime = Time.time;
        isAttackingAnimationPlaying = true; // Set the flag immediately
    }

    // In EnemyAIStateMachine (or a new AnimationEventHandler component on the enemy GameObject):
    // public void OnBasicAttackAnimationEnd()
    // {
    //     // This function is called by the animation event at the end of the BasicAttack animation.
    //     // This handles the transition *out* of the attack animation's "hold".
    //     stateMachine.ChangeState(stateMachine.aggressiveState); // Re-enter aggressive to reset logic
    // }

    public override void OnDamageTaken()
    {
        // Ensure that taking damage interrupts the attack if needed.
        if (isAttackingAnimationPlaying)
        {
            if (animator != null)
            {
                animator.SetBool("IsAttacking", false); // If you were using a bool for attacking
                animator.ResetTrigger("BasicAttack"); // Useful to reset trigger if it might cause issues on re-entry
            }
            isAttackingAnimationPlaying = false;
        }

        if (enemyHealth.HealthPercentage <= stateMachine.CriticalHPThreshold)
        {
            stateMachine.ChangeState(stateMachine.getHitState);
        }
        else
        {
            stateMachine.ChangeState(stateMachine.getHitState);
        }
    }
}