using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class StaffLineRenderer : MonoBehaviour
{
    public RectTransform staffPanel;
    public GameObject linePrefab;
    public float staffHeight = 150f;
    public float lineThickness = 7f;

#if UNITY_EDITOR
    private bool needsRedraw = false;

    private void OnValidate()
    {
        if (!UnityEngine.Application.isPlaying)
        {
            needsRedraw = true;
            EditorApplication.update -= DelayedRedraw;
            EditorApplication.update += DelayedRedraw;
        }
    }

    private void DelayedRedraw()
    {
        EditorApplication.update -= DelayedRedraw;

        if (!this || !staffPanel || !linePrefab) return;

        ClearChildren();
        DrawStaffLines();
        needsRedraw = false;
    }
#endif

    private void Start()
    {
        if (UnityEngine.Application.isPlaying)
        {
            ClearChildren();
            DrawStaffLines();
        }
    }

    private void DrawStaffLines()
    {
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
            rt.anchoredPosition = new Vector2(0, Mathf.Round(rawY));
        }
    }

    private void ClearChildren()
    {
        for (int i = staffPanel.childCount - 1; i >= 0; i--)
        {
            Transform child = staffPanel.GetChild(i);
            if (child.name.StartsWith(linePrefab.name)) // 🎯 이름 비교로 오선만 삭제
            {
#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
                Object.DestroyImmediate(child.gameObject);
            else
#endif
                Destroy(child.gameObject);
            }
        }
    }

}
