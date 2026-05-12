using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CodeMonkey.HealthSystemCM;

public class EarthElement : MonoBehaviour, IElement
{
    private List<Vector2> drawnPoints = new List<Vector2>();
    private LineRenderer lineRenderer;
    private float currentMass;
    private float holdStartTime;

    [Header("Visuals")]
    [SerializeField] private GameObject pebblePrefab;
    [SerializeField] private MeshFilter boulderMeshFilter;
    [SerializeField] private MeshRenderer boulderMeshRenderer;

    [Header("Settings")]
    [SerializeField] private float floatTimer = 1f;
    [SerializeField] private float destroyTimer = 15f;
    [SerializeField] private float minimumDistanceBetweenPoints = 1f;
    [SerializeField] private float massMultiplier = 1f;
    [SerializeField] private float pebbleTimer = 0.1f;

    [Header("Damage stats")]
    [SerializeField] private float damageMultiplier = 0.1f;
    [SerializeField] private float minimumImpactVelocity = 3f;
    [SerializeField] private float minimumDamage = 0.5f;
    [SerializeField] private float maximumDamage = 50f;

    public LayerMask groundLayer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void OnHoldStart(Vector2 position)
    {
        drawnPoints.Clear();
        drawnPoints.Add(position);
        holdStartTime = Time.time;
    }

    public void OnHoldUpdate(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, groundLayer);
        if (hit.collider != null) return; //pauses the drawing if over obstacle.

        if (drawnPoints.Count == 0) return;
        Vector2 lastPoint = drawnPoints[drawnPoints.Count - 1];
        

        if (Vector2.Distance(position, lastPoint) >= minimumDistanceBetweenPoints)
        {
            drawnPoints.Add(position);
            lineRenderer.positionCount = drawnPoints.Count;
            Vector3[] positions = drawnPoints.Select(p => (Vector3)p).ToArray();
            lineRenderer.SetPositions(positions);
        }

    }

    public void OnHoldEnd(Vector2 position)
    {
        if (Time.time - holdStartTime < pebbleTimer)
        {
            StartCoroutine (PebbleSequence(position));
            return;
        }

        List<Vector2> hull = ConvexHull.Compute(drawnPoints);

        if (hull.Count < 3)
        {
            Destroy(gameObject);
            return;
        }

        PolygonCollider2D col = gameObject.AddComponent<PolygonCollider2D>();
        col.SetPath(0, hull.ToArray());
        lineRenderer.enabled = false;
        GenerateMesh(hull);

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; //floating initially upon spawning
        float area = CalculateArea(hull);
        rb.mass = area * massMultiplier;
        currentMass = rb.mass;
        Debug.Log($"Boulder spawned with a mass of {rb.mass}");
        StartCoroutine(BoulderSequence(rb));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float damage = Mathf.Clamp(currentMass * damageMultiplier, minimumDamage, maximumDamage);
        float impactSpeed = collision.relativeVelocity.magnitude;

        if (!collision.gameObject.CompareTag("Player")) return;
        if (impactSpeed < minimumImpactVelocity) return;


        /* SECTION BELOW ONLY APPLIES DAMAGE IF PLAYER IS HIT FROM ABOVE */
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) //normal pointing upwards
            {
                HealthSystemComponent hsc = collision.gameObject.GetComponent<HealthSystemComponent>();
                if (hsc != null)
                {
                    hsc.GetHealthSystem().Damage(damage);
                    Debug.Log($"Player1 took {damage} damage.");
                }
                break;
            }
        }
    }

    private IEnumerator PebbleSequence(Vector2 position)
    {
        Debug.Log("PebbleSequence started.");
        lineRenderer.enabled = false;
        GameObject pebbleInstance = Instantiate(pebblePrefab, position, Quaternion.identity);
        Rigidbody2D rb = pebbleInstance.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; //floating initially upon spawning
        yield return new WaitForSeconds(floatTimer);
        rb.gravityScale = 1;
        yield return new WaitForSeconds(destroyTimer);
        Destroy(pebbleInstance);
        Destroy(gameObject);
    }
    private IEnumerator BoulderSequence(Rigidbody2D rb)
    {
        Debug.Log("BoulderSequence started.");
        yield return new WaitForSeconds(floatTimer);
        rb.gravityScale = 1;
        yield return new WaitForSeconds(destroyTimer);
        yield return StartCoroutine(FadeOut());
        Destroy(gameObject);
    }

    private void GenerateMesh(List<Vector2> hull)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = hull.Select(p => (Vector3)p).ToArray();
        mesh.vertices = vertices;

        int[] triangles = new int[(hull.Count - 2) * 3];
        for (int i = 0; i < hull.Count - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        boulderMeshFilter.mesh = mesh;
    }

    private float CalculateArea(List<Vector2> hull)
    {
        float area = 0f;
        for (int i = 0; i < hull.Count; i++)
        {
            int j = (i + 1) % hull.Count;
            area += hull[i].x * hull[j].y;
            area -= hull[j].x * hull[i].y;
        }
        return Mathf.Abs(area) / 2f;
    }


    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float fadeDuration = 3f;
        Material mat = boulderMeshRenderer.material;
        Color startColor = mat.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            mat.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null; //wait one frame
        }
    }
}
