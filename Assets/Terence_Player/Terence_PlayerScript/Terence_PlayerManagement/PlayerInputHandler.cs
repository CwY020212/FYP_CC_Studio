using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions controls;
    
    // Input values that can be read by other scripts
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _dodgeInputThisFrame;
    private bool _lightAttackInputThisFrame;
    private bool _heavyAttackInputThisFrame;
    private bool _ultimateAttackInputThisFrame;
    private bool _skillAttackInputThisFrame;
    private bool _parryInputThisFrame;
    private bool _lockOnInputThisFrame;
    private bool _runToggleState;
    private bool _interactInputThisFrame;
    private bool _previousInputThisFrame; // Not currently used, consider removing if not needed
    private bool _nextInputThisFrame;     // Not currently used, consider removing if not needed

    // Internal flags to track button presses for a single frame
    private bool _lightAttackPressedLastFrame;
    private bool _heavyAttackPressedLastFrame;
    private bool _ultimateAttackPressedLastFrame;
    private bool _skillAttackPressedLastFrame;
    private bool _dodgePressedLastFrame;
    private bool _parryPressedLastFrame;
    private bool _lockOnPressedLastFrame;
    private bool _interactPressedLastFrame;
    private bool _previousPressedLastFrame;
    private bool _nextPressedLastFrame;

    private void Awake()
    {
        if (controls == null) 
        {
            controls = new InputSystem_Actions();
        }

        controls.Player.SetCallbacks(this);
    }
    public void OnEnable()
    {
        controls.Player.Enable();
    }
    public void OnDisable()
    {
        controls.Player.Disable();
    }

    // --- Input Callbacks (for Unity's Input System) ---
    // You would hook these up in your Input Action Asset
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed) // Button was pressed
        {
            _dodgeInputThisFrame = true;
        }
        else if (context.canceled) // Button was released
        {
            _dodgeInputThisFrame = false; // Reset for next press
        }
    }

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _lightAttackInputThisFrame = true;
        }
        else if (context.canceled)
        {
            _lightAttackInputThisFrame = false;
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _heavyAttackInputThisFrame = true;
        }
        else if (context.canceled)
        {
            _heavyAttackInputThisFrame = false;
        }
    }

    public void OnUltimateAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _ultimateAttackInputThisFrame = true;
        }
        else if (context.canceled)
        {
            _ultimateAttackInputThisFrame = false;
        }
    }

    public void OnSkillAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _skillAttackInputThisFrame = true;
        }
        else if (context.canceled)
        {
            _skillAttackInputThisFrame = false;
        }
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _parryInputThisFrame = true;
        }
        else if (context.canceled)
        {
            _parryInputThisFrame = false;
        }
    }

    public void OnLockOn(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _lockOnInputThisFrame = true;
        }
        else if (context.canceled)
        {
            _lockOnInputThisFrame = false;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        // Only toggle if the button was *pressed down* (performed phase)
        if (context.performed)
        {
            _runToggleState = !_runToggleState; // Flip the state
            // If you want to force run off when movement stops,
            // or if other actions interrupt, you'll handle that in the state machine.
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _interactInputThisFrame = true;
        }
        else if (context.canceled)
        {
            _interactInputThisFrame = false;
        }
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        if (context.performed) { _previousInputThisFrame = true; }
        else if (context.canceled) { _previousInputThisFrame = false; }
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (context.performed) { _nextInputThisFrame = true; }
        else if (context.canceled) { _nextInputThisFrame = false; }
    }


    // --- Public Getters for State Machine ---
    // These methods provide the state of the input, clearing one-shot inputs after reading.
    public Vector2 GetMoveInput() { return _moveInput; }
    public Vector2 GetLookInput() { return _lookInput; }

    public bool GetDodgeInputDown() // For a single frame press
    {
        bool result = _dodgeInputThisFrame && !_dodgePressedLastFrame;
        _dodgePressedLastFrame = _dodgeInputThisFrame;
        return result;
    }
    public bool GetDodgeInputHeld() { return _dodgeInputThisFrame; } // For holding the button

    public bool GetLightAttackInputDown()
    {
        bool result = _lightAttackInputThisFrame && !_lightAttackPressedLastFrame;
        _lightAttackPressedLastFrame = _lightAttackInputThisFrame;
        return result;
    }
    public bool GetHeavyAttackInputDown()
    {
        bool result = _heavyAttackInputThisFrame && !_heavyAttackPressedLastFrame;
        _heavyAttackPressedLastFrame = _heavyAttackInputThisFrame;
        return result;
    }
    public bool GetUltimateAttackInputDown()
    {
        bool result = _ultimateAttackInputThisFrame && !_ultimateAttackPressedLastFrame;
        _ultimateAttackPressedLastFrame = _ultimateAttackInputThisFrame;
        return result;
    }
    public bool GetSkillAttackInputDown()
    {
        bool result = _skillAttackInputThisFrame && !_skillAttackPressedLastFrame;
        _skillAttackPressedLastFrame = _skillAttackInputThisFrame;
        return result;
    }

    public bool GetParryInputDown()
    {
        bool result = _parryInputThisFrame && !_parryPressedLastFrame;
        _parryPressedLastFrame = _parryInputThisFrame;
        return result;
    }
    public bool GetLockOnInputDown()
    {
        bool result = _lockOnInputThisFrame && !_lockOnPressedLastFrame;
        _lockOnPressedLastFrame = _lockOnInputThisFrame;
        return result;
    }
    public bool GetRunInputHeld() { return _runToggleState; }
    public void DisableRunToggle()
    {
        _runToggleState = false;
    }
    public bool GetInteractInputDown()
    {
        bool result = _interactInputThisFrame && !_interactPressedLastFrame;
        _interactPressedLastFrame = _interactInputThisFrame;
        return result;
    }
    public bool GetPreviousInputDown()
    {
        bool result = _previousInputThisFrame && !_previousPressedLastFrame;
        _previousPressedLastFrame = _previousInputThisFrame;
        return result;
    }
    public bool GetNextInputDown()
    {
        bool result = _nextInputThisFrame && !_nextPressedLastFrame;
        _nextPressedLastFrame = _nextInputThisFrame;
        return result;
    }
}
