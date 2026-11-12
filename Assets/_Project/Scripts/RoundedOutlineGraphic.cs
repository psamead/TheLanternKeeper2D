using UnityEngine;
using UnityEngine.UI;

/// <summary>Thin rounded rectangle outline used by the in-world interaction prompt.</summary>
[AddComponentMenu("Lantern Keeper/UI/Rounded Outline Graphic")]
public sealed class RoundedOutlineGraphic : MaskableGraphic
{
    [SerializeField, Min(0.5f)] private float thickness = 2f;
    [SerializeField, Min(1f)] private float cornerRadius = 8f;
    [SerializeField, Range(2, 16)] private int cornerSegments = 6;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float radius = Mathf.Min(cornerRadius, Mathf.Min(rect.width, rect.height) * 0.5f);
        float half = thickness * 0.5f;
        Vector2[] centers =
        {
            new Vector2(rect.xMax - radius, rect.yMin + radius),
            new Vector2(rect.xMax - radius, rect.yMax - radius),
            new Vector2(rect.xMin + radius, rect.yMax - radius),
            new Vector2(rect.xMin + radius, rect.yMin + radius)
        };
        float[] starts = { -90f, 0f, 90f, 180f };
        int pointsPerCorner = cornerSegments + 1;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        int index = 0;
        for (int corner = 0; corner < 4; corner++)
        {
            for (int segment = 0; segment <= cornerSegments; segment++)
            {
                float angle = (starts[corner] + 90f * segment / cornerSegments) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 outer = centers[corner] + direction * radius;
                Vector2 inner = centers[corner] + direction * Mathf.Max(0f, radius - thickness);
                vertex.position = outer; vh.AddVert(vertex);
                vertex.position = inner; vh.AddVert(vertex);
                if (index > 0)
                {
                    int previous = index * 2 - 2;
                    vh.AddTriangle(previous, previous + 1, previous + 2);
                    vh.AddTriangle(previous + 2, previous + 1, previous + 3);
                }
                index++;
            }
        }
        // Bridge each corner's arc to the next one with the straight edge.
        for (int corner = 0; corner < 4; corner++)
        {
            int last = (corner * pointsPerCorner + cornerSegments) * 2;
            int next = ((corner + 1) % 4 * pointsPerCorner) * 2;
            vh.AddTriangle(last, last + 1, next);
            vh.AddTriangle(next, last + 1, next + 1);
        }
    }
}
