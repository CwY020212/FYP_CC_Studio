using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(PlayerInputHandler), typeof(CharacterController), typeof(Animator))]
[RequireComponent(typeof(StaminaSystem), typeof(HealthSystem), typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerWeaponManager),typeof(PlayerInteractionController))]
// You would also create and add an InteractionController script
public class PlayerStateMachine : MonoBehaviour
{
    // States (These are now just instances, the logic is inside their classes)
    [Header("States")]
    public PlayerBaseState currentState;
    public PlayerIdleState idleState;
    public PlayerMovementState movementState;
    public PlayerRunState runState;
    public PlayerDodgeState dodgeState;
    public PlayerAttackState attackState;
    public PlayerParryState parryState;
    public PlayerLockOnState lockOnState;
    public PlayerInteractState interactState;
    public PlayerFallingState fallingState;
    public PlayerGetHitState getHitState;

    // --- References to New Components ---
    [Header("Core Components")]
    public PlayerInputHandler inputHandler { get; private set; }
    public PlayerMotor Motor { get; private set; }
    public Animator animator { get; private set; }
    public PlayerWeaponManager weaponManager { get; private set; }
    // Add other managers/controllers as you create them (Interaction, Damage, etc.)
    public StaminaSystem staminaSystem { get; private set; }
    public HealthSystem healthSystem { get; private set; }
    public CharacterController characterController { get; private set; }
    public PlayerRagdollController playerRagdollController { get; private set; }
    public PlayerInteractionController interactionController { get; private set; }
    public IInteractable currentTargetInteractable { get; set; }

    [Header("Movement Speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("Dodge Settings")]
    public float rollingDuration = 0.5f;
    public float rollingSpeed;

    public event System.Action OnInteractionAnimationEnd;

    private void Awake()
    {
        // Get references to all components
        inputHandler = GetComponent<PlayerInputHandler>();
        Motor = GetComponent<PlayerMotor>();
        animator = GetComponent<Animator>();
        weaponManager = GetComponent<PlayerWeaponManager>(); // Changed from WeaponManager to PlayerWeaponManager
        staminaSystem = GetComponent<StaminaSystem>();
        healthSystem = GetComponent<HealthSystem>();
        characterController = GetComponent<CharacterController>();
        playerRagdollController = GetComponent<PlayerRagdollController>();
        interactionController = GetComponent<PlayerInteractionController>();

        // Initialize all state instances, passing 'this' as the context
        idleState = new PlayerIdleState(this);
        movementState = new PlayerMovementState(this);
        runState = new PlayerRunState(this);
        dodgeState = new PlayerDodgeState(this);
        attackState = new PlayerAttackState(this);
        parryState = new PlayerParryState(this);
        fallingState = new PlayerFallingState(this);
        getHitState = new PlayerGetHitState(this);
        interactState = new PlayerInteractState(this);

        // Set the initial state
        currentState = idleState;
        currentState.EnterState();
    }

    private void Update()
    {
        // The entire Update loop is now just this!
        // The current state itself handles all its logic and transitions.
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }

    public void SwitchState(PlayerBaseState newState)
    {
        if (currentState == newState) return;

        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    // Animation events can remain here as they are called on the GameObject
    public void OnAttackAnimationEnd()
    {
        // Forward the event to the current state, if it's the right one
        if (currentState == attackState)
        {
            attackState.OnAttackAnimationEnd();
        }
    }

    // A public method for other systems (like a DamageHandler) to call
    public void TakeHit(HitData hitData) // Assuming HitData is a defined struct/class
    {
        // You can still have logic here to prevent getting hit in certain states
        if (currentState == dodgeState) return;

        getHitState.SetHitData(hitData); // Assuming SetHitData exists on PlayerGetHitState
        SwitchState(getHitState);
    }
   
    // This method will be called by an Animation Event on the player's Animator
    public void AnimationEvent_InteractionComplete()
    {
        Debug.Log("Animation Event: InteractionComplete fired!");
        // Trigger the event so the current state (PlayerInteractState) can listen
        OnInteractionAnimationEnd?.Invoke();
    }
}

