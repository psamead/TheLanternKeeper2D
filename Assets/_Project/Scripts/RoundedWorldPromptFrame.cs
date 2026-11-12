using TMPro;
using UnityEngine;

/// <summary>Draws a thin rounded outline around a world-space TextMeshPro prompt.</summary>
[RequireComponent(typeof(TMP_Text), typeof(LineRenderer))]
public sealed class RoundedWorldPromptFrame : MonoBehaviour
{
    [SerializeField, Min(0f)] private float padding = 0.12f;
    [SerializeField, Min(0.001f)] private float lineWidth = 0.018f;
    [SerializeField, Min(0f)] private float radius = 0.08f;
    [SerializeField, Range(2, 16)] private int segmentsPerCorner = 5;
    private TMP_Text label;
    private LineRenderer line;
    private Vector2 lastPreferredSize;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.widthMultiplier = lineWidth;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingLayerID = GetComponent<Renderer>().sortingLayerID;
        line.sortingOrder = GetComponent<Renderer>().sortingOrder + 1;
        Rebuild(true);
    }

    private void LateUpdate()
    {
        line.startColor = line.endColor = label.color;
        Rebuild(false);
    }

    private void OnDestroy()
    {
        if (line && line.material) Destroy(line.material);
    }

    private void Rebuild(bool force)
    {
        if (!label || !line) return;
        label.ForceMeshUpdate();
        Vector2 preferred = label.GetPreferredValues(label.text);
        if (!force && (preferred - lastPreferredSize).sqrMagnitude < 0.001f) return;
        lastPreferredSize = preferred;
        RectTransform rect = label.rectTransform;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferred.x + padding * 2f);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferred.y + padding * 2f);
        float halfW = rect.rect.width * 0.5f;
        float halfH = rect.rect.height * 0.5f;
        float r = Mathf.Min(radius, Mathf.Min(halfW, halfH));
        Vector2[] centers =
        {
            new Vector2(halfW-r,-halfH+r), new Vector2(halfW-r,halfH-r),
            new Vector2(-halfW+r,halfH-r), new Vector2(-halfW+r,-halfH+r)
        };
        float[] starts = { -90f, 0f, 90f, 180f };
        Vector3[] points = new Vector3[4 * (segmentsPerCorner + 1)];
        int index = 0;
        for (int corner=0; corner<4; corner++)
            for (int segment=0; segment<=segmentsPerCorner; segment++)
            {
                float angle=(starts[corner]+90f*segment/segmentsPerCorner)*Mathf.Deg2Rad;
                points[index++]=centers[corner]+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r;
            }
        line.startColor = line.endColor = label.color;
        line.widthMultiplier = lineWidth;
        line.positionCount = points.Length;
        line.SetPositions(points);
    }
}
