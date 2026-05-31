using UnityEngine;

public class WaterElement : MonoBehaviour, IElement
{
    [SerializeField] private GameObject dropletPrefab;
    [SerializeField] private float spawnCooldown = 0f;
    [SerializeField] private float spawnRate = 0.5f;


    public void OnHoldStart(Vector2 position)
    {
        if (spawnCooldown <= 0)
        {
            SpawnDroplet(position);
            spawnCooldown = spawnRate;
        }
    }

    public void OnHoldUpdate(Vector2 position)
    {
        if (spawnCooldown <= 0)
        {
            SpawnDroplet(position);
            spawnCooldown = spawnRate;
        }
    }

    public void OnHoldEnd(Vector2 position)
    {

    }

    private void SpawnDroplet(Vector2 position)
    {
        GameObject dropletInstance = Instantiate(dropletPrefab, position, Quaternion.identity);
    }

    void Update()
    {
        if (spawnCooldown > 0) spawnCooldown -= Time.deltaTime;
    }

}
