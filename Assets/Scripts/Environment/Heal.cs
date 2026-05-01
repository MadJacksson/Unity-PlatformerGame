using CodeMonkey.HealthSystemCM;
using UnityEngine;

public class Heal : MonoBehaviour
{

    [SerializeField] private float healAmount = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HealthSystemComponent healthSystemComponent = collision.GetComponent<HealthSystemComponent>();
            if (healthSystemComponent != null)
            {
                healthSystemComponent.GetHealthSystem().Heal(healAmount);
                Destroy(gameObject);
            }

        }
    }
}
