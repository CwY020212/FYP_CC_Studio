using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    private CharacterController _characterController;
    public Transform _cameraTransform;

    [Header("Movement Settings")]
    public float rotationSpeed = 10f;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float groundedPushForce = -0.5f;
    private Vector3 _verticalVelocity;

    [Header("Ground Check Settings")]
    public LayerMask groundCheckLayer;
    [Range(0.01f, 1f)]
    public float groundCheckDistance = 0.2f;
    public float groundCheckRadiusMultiplier = 0.9f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        // Apply gravity. Movement is now driven by states.
        if (IsGrounded())
        {
            _verticalVelocity.y = groundedPushForce;
        }
        else
        {
            _verticalVelocity.y += gravity * Time.fixedDeltaTime;
        }

        // Apply only vertical velocity unless Move() is called
        // If there's no horizontal movement from the states, this will still apply gravity.
        if (!IsGrounded() || _verticalVelocity.y != groundedPushForce) // Only move with vertical velocity if not grounded or if gravity is actively applied
        {
            _characterController.Move(_verticalVelocity * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Moves the character horizontally based on input and current speed.
    /// This should be called by the current state (e.g., MovementState, RunState).
    /// </summary>
    public void Move(Vector2 moveInput, float speed)
    {
        Vector3 horizontalMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        horizontalMoveDirection = _cameraTransform.TransformDirection(horizontalMoveDirection);
        horizontalMoveDirection.y = 0f;
        horizontalMoveDirection.Normalize();

        Vector3 movement = horizontalMoveDirection * speed;

        // Combine with existing vertical velocity and apply
        _characterController.Move((movement + _verticalVelocity) * Time.fixedDeltaTime);

        Rotate(horizontalMoveDirection);
    }

    /// <summary>
    /// Rotates the character to face the move direction.
    /// </summary>
    public void Rotate(Vector3 direction)
    {
        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public bool IsGrounded()
    {
        // Calculate the origin of the sphere for ground check
        // It's slightly above the bottom of the character controller's capsule to detect ground contact
        Vector3 sphereOrigin = transform.position + _characterController.center - Vector3.up * (_characterController.height / 2f - _characterController.radius + groundCheckDistance);
        float effectiveGroundCheckRadius = _characterController.radius * groundCheckRadiusMultiplier;

        // Use the specified groundCheckLayer to only detect ground
        return Physics.CheckSphere(sphereOrigin, effectiveGroundCheckRadius, groundCheckLayer);
    }

    // Optional: Gizmos for debugging
    private void OnDrawGizmos()
    {
        if (_characterController == null) _characterController = GetComponent<CharacterController>();
        // Ensure the origin calculation matches the IsGrounded method for consistency
        Vector3 sphereOrigin = transform.position + _characterController.center - Vector3.up * (_characterController.height / 2f - _characterController.radius + groundCheckDistance);
        float effectiveGroundCheckRadius = _characterController.radius * groundCheckRadiusMultiplier;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(sphereOrigin, effectiveGroundCheckRadius);
    }
}