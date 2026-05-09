using CodeMonkey.HealthSystemCM;
using System.Collections;
using UnityEngine;

public class LightningElement : MonoBehaviour, IElement
{
    [SerializeField] private GameObject warningIndicatorPrefab;
    [SerializeField] private float travelTime;
    [SerializeField] private float flashDuration;

    [Header("Ability stats")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float areaOfEffect;

    public LayerMask groundLayer;

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
        GameObject warningInstance = Instantiate(warningIndicatorPrefab, groundHit.point, Quaternion.identity);

        yield return new WaitForSeconds(travelTime);

        Destroy(warningInstance);
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
}
