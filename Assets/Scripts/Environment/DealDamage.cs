using CodeMonkey.HealthSystemCM;
using UnityEngine;
using UnityEngine.Audio;

public class DealDamage : MonoBehaviour
{

    [SerializeField] private float damageAmount = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HealthSystemComponent healthSystemComponent = collision.GetComponent<HealthSystemComponent>();
            if (healthSystemComponent != null)
            {
                healthSystemComponent.GetHealthSystem().Damage(damageAmount);
                Destroy(gameObject);
            }

        }
    }
}
