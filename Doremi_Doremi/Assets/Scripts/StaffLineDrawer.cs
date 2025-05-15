using UnityEngine;
using UnityEngine.UI;

public class StaffLineDrawer : MonoBehaviour
{
    [Header("🎵 오선을 그릴 대상 패널")]
    public RectTransform staffPanel;

    [Header("🎵 줄 프리팹")]
    public GameObject linePrefab;

private void Start()
    {
        DrawOneCentralLine();
    }

 private void DrawOneCentralLine()
    {
        if (staffPanel == null || linePrefab == null)
        {
            Debug.LogError("StaffPanel 또는 LinePrefab이 설정되지 않았습니다!");
            return;
        }

        float staffPanelHeight = staffPanel.rect.height;
        float spacing = staffPanelHeight / 10f;
        float thickness = Mathf.Max(spacing * 0.1f, 2f);

        GameObject line = Instantiate(linePrefab, staffPanel);
        RectTransform line_rt = line.GetComponent<RectTransform>();

        line_rt.anchorMin = new Vector2(0f, 0.5f);
        line_rt.anchorMax = new Vector2(1f, 0.5f);
        line_rt.pivot = new Vector2(0.5f, 0.5f);
        line_rt.sizeDelta = new Vector2(0, thickness);
        line_rt.anchoredPosition = new Vector2(0, 0);  // 중앙 줄
    }

}
