using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput : MonoBehaviour, PlayerControls.IPlayerActions
{
    public PlayerControls PlayerControls { get; private set; }

    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public event System.Action OnPausePressed;

    // NEW: input lock flag
    public bool inputLocked = false;

    // NEW: public method to lock/unlock inputs
    public void LockInput(bool locked)
    {
        inputLocked = locked;

        if (locked)
        {
            // Immediately clear any current input
            MovementInput = Vector2.zero;
            LookInput = Vector2.zero;
        }
    }

    private void OnEnable()
    {
        PlayerControls = new PlayerControls();
        PlayerControls.Enable();

        PlayerControls.Player.Enable();
        PlayerControls.Player.SetCallbacks(this);
    }

    private void OnDisable()
    {
        PlayerControls.Player.Disable();
        PlayerControls.Player.RemoveCallbacks(this);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (inputLocked)
        {
            MovementInput = Vector2.zero;
            return;
        }

        MovementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (inputLocked)
        {
            LookInput = Vector2.zero;
            return;
        }

        LookInput = context.ReadValue<Vector2>();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnPausePressed?.Invoke();
    }
}
