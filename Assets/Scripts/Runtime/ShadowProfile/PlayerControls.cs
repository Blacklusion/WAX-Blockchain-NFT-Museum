using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    private InputAction movement;
    private InputAction look;

    public InputAction PlayerMovement => movement;
    public InputAction PlayerLook => look;

    private void Awake()
    {
        movement = new InputAction("PlayerMovement", InputActionType.Value);
        look = new InputAction("PlayerLook", InputActionType.Value);
    }

    private void OnEnable()
    {
        movement.Enable();
        look.Enable();
    }

    private void OnDisable()
    {
        movement.Disable();
        look.Disable();
    }
}