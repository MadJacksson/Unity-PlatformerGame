using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player2Controller : MonoBehaviour
{
    private InputAction leftMouseClick;
    private InputAction rightMouseClick;
    private ElementType leftElement = ElementType.None;
    private ElementType rightElement = ElementType.None;
    private IElement leftElementInstance;
    private IElement rightElementInstance;
    private GameObject leftElementPrefab;
    private GameObject rightElementPrefab;

    [SerializeField] private Transform cursorIndicator;

    [Header("Element Prefabs")]
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private GameObject lavaPrefab;
    [SerializeField] private GameObject earthPrefab;
    [SerializeField] private GameObject windPrefab;
    [SerializeField] private GameObject lightningPrefab;

    void Start()
    {
        InputActionAsset inputActions = GetComponent<PlayerInput>().actions;
        leftMouseClick = inputActions.FindAction("PrimaryInteract");
        rightMouseClick = inputActions.FindAction("SecondaryInteract");
    }

    void Update()
    {
        Vector2 worldPosition = CheckMousePosition();

        if (leftMouseClick.WasPressedThisFrame())
        {
            Debug.Log($"Primary interact triggered at {worldPosition}.");
            Interact(worldPosition, true);
        }
        if (rightMouseClick.WasPressedThisFrame())
        {
            Debug.Log($"Secondary interact trigered at {worldPosition}.");
            Interact(worldPosition, false);
        }

        if (!leftMouseClick.WasPressedThisFrame() && leftMouseClick.IsPressed() && leftElementInstance != null)
        {
            leftElementInstance.OnHoldUpdate(worldPosition);
        } 
        if (!rightMouseClick.WasPressedThisFrame() && rightMouseClick.IsPressed() && rightElementInstance != null)
        {
            rightElementInstance.OnHoldUpdate(worldPosition);
        }

        if (leftMouseClick.WasReleasedThisFrame() && leftElementInstance != null)
        {
            Debug.Log("Primary interact was released this frame.");
            leftElementInstance.OnHoldEnd(worldPosition);
        }
        if (rightMouseClick.WasReleasedThisFrame() && rightElementInstance != null)
        {
            Debug.Log("Secondary interact was released this frame.");
            rightElementInstance.OnHoldEnd(worldPosition);
        }
    }

    public Vector2 CheckMousePosition()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        cursorIndicator.position = worldPosition;
        return worldPosition;
    }

    private void Interact(Vector2 position, bool isPrimary)
    {
        ElementType detectedElement = ElementType.None;
        GameObject selectedPrefab = null;

        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);

        if (hit.collider == null)
        {
            IElement equippedInstance = isPrimary ? leftElementInstance : rightElementInstance;
            GameObject equippedPrefab = isPrimary ? leftElementPrefab : rightElementPrefab;
            Debug.Log($"Prefab assigned: {leftElementPrefab != null}, Instance assigned: {leftElementInstance != null}");
            if (equippedInstance == null || equippedPrefab == null)
            {
                Debug.Log("Nothing equipped.");
                return;
            }

            GameObject spawnedObject = Instantiate(equippedPrefab, position, Quaternion.identity);
            IElement spawnedInstance = spawnedObject.GetComponent<IElement>();
            if (isPrimary) leftElementInstance = spawnedInstance;
            else rightElementInstance = spawnedInstance;
            spawnedInstance.OnHoldStart(position);
            return;
        }

        switch (hit.collider.gameObject.tag)
        {
            case "Water": detectedElement = ElementType.Water; break;
            case "Lava": detectedElement = ElementType.Lava; break;
            case "Earth": detectedElement = ElementType.Earth; break;
            case "Wind": detectedElement = ElementType.Wind; break;
            case "Lightning": detectedElement = ElementType.Lightning; break;
        }

        switch (detectedElement)
        {
            case ElementType.Water: selectedPrefab = waterPrefab; break;
            case ElementType.Lava: selectedPrefab = lavaPrefab; break;
            case ElementType.Earth: selectedPrefab = earthPrefab; break;
            case ElementType.Wind: selectedPrefab = windPrefab; break;
            case ElementType.Lightning: selectedPrefab = lightningPrefab; break;
        }

        if (detectedElement != ElementType.None)
        {
            if (isPrimary)
            {
                leftElement = detectedElement;
                leftElementPrefab = selectedPrefab;
            }
            else
            {
                rightElement = detectedElement;
                rightElementPrefab = selectedPrefab;
            }
            Debug.Log($"{detectedElement} equipped in {(isPrimary ? "Primary" : "Secondary")}.");
        }
    }
}
