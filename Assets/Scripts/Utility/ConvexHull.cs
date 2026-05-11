using UnityEngine;
using System.Collections.Generic;

public static class ConvexHull
{
    public static List<Vector2> Compute(List<Vector2> points)
    {
        if (points.Count < 3)
        {
            return new List<Vector2>();
        }

        int startIndex = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < points[startIndex].x)
            {
                startIndex = i;
            }
        }

        List<Vector2> hull = new List<Vector2>();
        int current = startIndex;

        do
        {
            hull.Add(points[current]);
            int next = 0;

            for (int i = 1; i < points.Count; i++)
            {
                if (next == current || Cross(points[current], points[next], points[i]) < 0)
                {
                    next = i;
                }
            }
            current = next;                
        }
        while (current != startIndex);

        return hull;
    }

    private static float Cross(Vector2 origin, Vector2 a, Vector2 b)
    {
        return (a.x - origin.x) * (b.y - origin.y) - (a.y - origin.y) * (b.x - origin.x);
    }
}
