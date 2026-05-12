using CodeMonkey.HealthSystemCM;
using System.Collections;
using System.Net;
using UnityEngine;

public class LightningElement : MonoBehaviour, IElement
{
    private LineRenderer lightningRenderer;

    [Header("Visuals")]
    [SerializeField] private GameObject warningIndicatorPrefab;
    [SerializeField] private float warningIndicatorYOffset = 1f;
    [SerializeField] private float travelTime;
    [SerializeField] private float flashDuration;
    [SerializeField] private float lightningBoltOffsetX = 1f;


    [Header("Element stats")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float areaOfEffect;

    public LayerMask groundLayer;

    private void Start()
    {
        lightningRenderer = GetComponent<LineRenderer>();
    }

    public void OnHoldStart(Vector2 position)
    {
        StartCoroutine(LightningSequence(position));
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


    private IEnumerator LightningSequence(Vector2 position)
    {
        RaycastHit2D groundHit = Physics2D.Raycast(position, Vector2.down, Mathf.Infinity, groundLayer);
        if (groundHit.collider == null) yield break;
        Vector2 warningPosition = new Vector2(groundHit.point.x, groundHit.point.y + warningIndicatorYOffset);
        GameObject warningInstance = Instantiate(warningIndicatorPrefab, warningPosition, Quaternion.identity);

        yield return new WaitForSeconds(travelTime);

        Destroy(warningInstance);
        DrawLightningBolt(position, groundHit.point);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(groundHit.point, areaOfEffect);
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                HealthSystemComponent healthSystemComponent = collider.GetComponent<HealthSystemComponent>();
                if (healthSystemComponent != null)
                {
                    healthSystemComponent.GetHealthSystem().Damage(damageAmount);
                    Debug.Log($"Player1 took {damageAmount} damage.");
                }
            }
        }

        yield return new WaitForSeconds(flashDuration);
        Destroy(gameObject);
    }

    private void DrawLightningBolt(Vector2 startPoint, Vector2 endPoint)
    {
        Debug.Log($"Linerenderer is null: {lightningRenderer == null}");
        
        Vector2 midPoint1 = Vector2.Lerp(startPoint, endPoint, 0.33f);
        midPoint1.x += Random.Range(-lightningBoltOffsetX, lightningBoltOffsetX);

        Vector2 midPoint2 = Vector2.Lerp(startPoint, endPoint, 0.66f);
        midPoint2.x += Random.Range(-lightningBoltOffsetX, lightningBoltOffsetX);

        lightningRenderer.positionCount = 4;
        lightningRenderer.SetPosition(0, startPoint);
        lightningRenderer.SetPosition(1, midPoint1);
        lightningRenderer.SetPosition(2, midPoint2);
        lightningRenderer.SetPosition(3, endPoint);

        Debug.Log($"Drawing bolt from {startPoint} to {endPoint}.");
    }
}
