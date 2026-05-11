using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EarthElement : MonoBehaviour
{
    private List<Vector2> drawnPoints = new List<Vector2>();
    private LineRenderer lineRenderer;

    [Header("Visuals")]
    [SerializeField] private GameObject pebblePrefab;

    [SerializeField] private float floatTimer = 1f;
    [SerializeField] private float destroyTimer = 30f;
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
        
    }

}
