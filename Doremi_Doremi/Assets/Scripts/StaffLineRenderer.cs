using UnityEngine;

[ExecuteAlways] // 💡 이 줄 추가!
public class StaffLineRenderer : MonoBehaviour
{
    public RectTransform staffPanel;
    public GameObject linePrefab;
    public float staffHeight = 150f;
    public float lineThickness = 7f;

    private void Start()
    {
        DrawStaffLines();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 파라미터 바뀔 때도 자동 반영
        if (!Application.isPlaying)
        {
            ClearChildren();
            DrawStaffLines();
        }
    }
#endif

    private void DrawStaffLines()
    {
        if (staffPanel == null || linePrefab == null) return;

        int lineCount = 5;
        float spacing = staffHeight / (lineCount - 1);
        float baseY = Mathf.Round(staffPanel.anchoredPosition.y);

        for (int i = 0; i < lineCount; i++)
        {
            GameObject line = Instantiate(linePrefab, staffPanel);
            RectTransform rt = line.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, lineThickness);

            float rawY = baseY + staffHeight - i * spacing;
            float yPos = Mathf.Round(rawY);
            rt.anchoredPosition = new Vector2(0, yPos);
        }
    }

    private void ClearChildren()
    {
        for (int i = staffPanel.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(staffPanel.GetChild(i).gameObject);
        }
    }
}
