using UnityEngine;
using UnityEngine.AI; 

public abstract class EnemyBaseState
{
    // Protected reference to the state machine context
    protected EnemyAIStateMachine stateMachine;
    protected NavMeshAgent agent;
    protected Transform playerTransform;
    protected Animator animator;
    protected IEnemyHealth enemyHealth; // Using the interface here!

    // Constructor to inject dependencies
    public EnemyBaseState(EnemyAIStateMachine stateMachine, NavMeshAgent agent, Transform playerTransform, Animator animator, IEnemyHealth enemyHealth)
    {
        this.stateMachine = stateMachine;
        this.agent = agent;
        this.playerTransform = playerTransform;
        this.animator = animator;
        this.enemyHealth = enemyHealth;
    }

    // Methods that all states MUST implement
    public abstract void EnterState();  // Called once when entering the state
    public abstract void UpdateState(); // Called every frame while in the state
    public abstract void ExitState();   // Called once when exiting the state

    // Optional: for physics updates
    // public virtual void FixedUpdateState() { }
    // Optional: for handling specific events like damage taken
    public virtual void OnDamageTaken() { }

    // Helper method for common state logic (e.g., facing target)
    protected void FaceTarget(Vector3 targetPosition)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return; // Ensure agent is valid

        Vector3 direction = (targetPosition - stateMachine.transform.position).normalized;
        if (direction.sqrMagnitude > 0.01f) // Avoid looking at (0,0,0) if target is at same position
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            stateMachine.transform.rotation = Quaternion.Slerp(stateMachine.transform.rotation, lookRotation, Time.deltaTime * agent.angularSpeed * 0.1f);
        }
    }
}
