using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EarthElement : MonoBehaviour, IElement
{
    private List<Vector2> drawnPoints = new List<Vector2>();
    private LineRenderer lineRenderer;

    [Header("Visuals")]
    [SerializeField] private GameObject pebblePrefab;

    [SerializeField] private float floatTimer = 1f;
    [SerializeField] private float destroyTimer = 15f;
    [SerializeField] private int minimumPointCount = 3;
    [SerializeField] private float minimumDistanceBetweenPoints = 1f;

    public LayerMask groundLayer;


    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void OnHoldStart(Vector2 position)
    {
        drawnPoints.Clear();
        drawnPoints.Add(position);
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
        if (drawnPoints.Count < minimumPointCount)
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

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; //floating initially upon spawning
        StartCoroutine(BoulderSequence(rb));
    }

    private IEnumerator PebbleSequence(Vector2 position)
    {
        GameObject pebbleInstance = Instantiate(pebblePrefab, position, Quaternion.identity);
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

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float fadeDuration = 3f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            Color startColor = lineRenderer.startColor;
            startColor.a = alpha;
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = startColor;
            yield return null; //wait one frame
        }
    }

}
