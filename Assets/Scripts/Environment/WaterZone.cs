using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class WaterZone : MonoBehaviour
{
    private BoxCollider2D col;
    private float evaporationTimer;
    private bool evaporating = false;

    [Header("Current")]
    public Vector2 currentDirection = Vector2.zero;
    public float currentStrength = 3f;

    [Header("Evaporation")]
    public float evaporationDelay = 5f;
    public float evaporationSpeed = 0.5f;

    //Will be called by WaterElement.cs to grow the zone downward
    public void AddWater(float heightToAdd)
    {
        //Grow the collider downward by adjusting size and offset.
        Vector2 size = col.size;
        Vector2 offset = col.offset;

        size.y += heightToAdd;
        offset.y -= heightToAdd / 2f; //anchor top edge, grow down

        col.size = size;
        col.offset = offset;

        SyncVisual();

        //reset evaporation timer every time water is added.
        evaporationTimer = evaporationDelay;
        evaporating = false;
    }

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        evaporationTimer = evaporationDelay;
    }

    private void Update()
    {
        evaporationTimer -= Time.deltaTime;

        if (evaporationTimer <= 0f) evaporating = true;
        if (evaporating) Evaporate();
    }

    private void Evaporate()
    {
        Vector2 size = col.size;
        Vector2 offset = col.offset;

        float shrink = evaporationSpeed * Time.deltaTime;
        size.y -= shrink;
        offset.y += shrink / 2f; //shrink from bottom up

        if (size.y <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        col.size= size;
        col.offset = offset;
        SyncVisual();
    }

    private void SyncVisual()
    {
        //Placeholder - later we'll drive a SpriteRenderer or mesh here
        //For now we'll attach a child sprite and scale it manually
    }
}
