using UnityEngine;
using UnityEngine.InputSystem;

public class Player2Controller : MonoBehaviour
{
    private InputAction leftMouseClick;
    private InputAction rightMouseClick;

    [SerializeField] private Transform cursorIndicator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InputActionAsset inputActions = GetComponent<PlayerInput>().actions;
        leftMouseClick = inputActions.FindAction("PrimaryInteract");
        rightMouseClick = inputActions.FindAction("SecondaryInteract");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 worldPosition = CheckMousePosition();

        if (leftMouseClick.WasPressedThisFrame())
        {
            Debug.Log($"Primary interact triggered at {worldPosition}.");
            Interact(worldPosition);
        }
        if (rightMouseClick.WasPressedThisFrame())
        {
            Debug.Log($"Secondary interact trigered at {worldPosition}.");
            Interact(worldPosition);
        }
    }

    public Vector2 CheckMousePosition()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        cursorIndicator.position = worldPosition;
        return worldPosition;
    }

    private void Interact(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider == null)
        {
            Debug.Log("Nothing to interact with.");
        } else
        {
            Debug.Log(hit.collider.gameObject.name);
        }
    }
}
