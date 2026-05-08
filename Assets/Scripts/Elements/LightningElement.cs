using UnityEngine;

public class LightningElement : MonoBehaviour, IElement
{
    [SerializeField] private GameObject warningIndicatorPrefab;
    [SerializeField] private float travelTime;
    [SerializeField] private float flashDuration;

    public void OnHoldStart(Vector2 position)
    {
        RaycastHit2D groundHit = Physics2D.Raycast(position, Vector2.down);
        Debug.Log("Lightning Triggered.");
    }

    public void OnHoldUpdate(Vector2 position)
    {
        // not used by Lightning
    }

    public void OnHoldEnd(Vector2 position)
    {
        // not used by Lightning
    }
}
