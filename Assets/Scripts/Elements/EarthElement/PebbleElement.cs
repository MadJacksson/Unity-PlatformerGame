using UnityEngine;
using CodeMonkey.HealthSystemCM;

public class PebbleElement : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float minimumImpactVelocity = 3f;
    [SerializeField] private float damageMultiplier = 0.1f;
    [SerializeField] private float minimumDamage = 0.5f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < minimumImpactVelocity) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                float damage = Mathf.Clamp(rb.mass * damageMultiplier, minimumDamage, Mathf.Infinity);
                HealthSystemComponent hsc = collision.gameObject.GetComponent<HealthSystemComponent>();
                if (hsc != null)
                {
                    hsc.GetHealthSystem().Damage(damage);
                    Debug.Log($"Player hit by pebble for {damage} damage.");
                }
                break;
            }
        }
    }

}
