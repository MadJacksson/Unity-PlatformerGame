using System.Linq;
using UnityEngine;

public class WaterDroplet : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasSettled = false;
    private Collider2D[] nearby;

    [SerializeField] private GameObject waterZonePrefab;
    [SerializeField] private float settleThreshold = 0.2f;
    [SerializeField] private float mergeDropletsRadius = 0.5f;
    [SerializeField] private float dropletTimer = 10f;
    [SerializeField] private float addWaterAmount = 1f;
    [SerializeField] private int mergeThreshold = 5;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Settle();

        dropletTimer -= Time.deltaTime;
        if (dropletTimer < 0) DestroyDroplet();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            MergeIntoZone(collision.gameObject.GetComponent<WaterZone>());
        }
    }

    private void Settle()
    {
        if (hasSettled) return;
        if (rb.linearVelocity.magnitude < settleThreshold)
        {
            hasSettled = true;
            nearby = Physics2D.OverlapCircleAll(rb.position, mergeDropletsRadius);
            int dropletCount = nearby.Count(c => c.CompareTag("Droplet"));
            if (dropletCount >= mergeThreshold) SpawnWaterZone();
        }
    }

    private void MergeIntoZone(WaterZone zone)
    {
        zone.AddWater(addWaterAmount);
        Destroy(gameObject);
    }

    private void SpawnWaterZone()
    {
        GameObject waterZoneInstance = Instantiate(waterZonePrefab, transform.position, Quaternion.identity);
        foreach (Collider2D c in nearby)
        {
            if (c.CompareTag("Droplet")) Destroy(c.gameObject);
        }
    }

    private void DestroyDroplet()
    {
        Destroy(gameObject);
    }
}
