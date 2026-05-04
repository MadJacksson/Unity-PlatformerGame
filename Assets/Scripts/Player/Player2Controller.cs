using UnityEngine;
using UnityEngine.InputSystem;

public class Player2Controller : MonoBehaviour
{
    private InputAction leftMouseClick;
    private InputAction rightMouseClick;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputActionAsset inputActions = GetComponent<PlayerInput>().actions;
        leftMouseClick = inputActions.FindAction("Attack");
        rightMouseClick = inputActions.FindAction("SecondaryAttack");
    }

    // Update is called once per frame
    void Update()
    {
        if (leftMouseClick.WasPressedThisFrame())
        {
            CheckMousePosition();
            Debug.Log("Primary attack triggered");
        }
        if (rightMouseClick.WasPressedThisFrame())
        {
            CheckMousePosition();
            Debug.Log("Secondary attack trigered.");
        }
    }

    public void CheckMousePosition()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
}
