using UnityEngine;  // Unity 핵심 클래스 및 메서드를 제공하는 네임스페이스
#if UNITY_EDITOR
using UnityEditor;  // 에디터 전용 API 사용을 위한 네임스페이스
#endif

[ExecuteAlways]  // 에디터와 플레이 모드 구분 없이 항상 실행되도록 지정
public class StaffLineRenderer : MonoBehaviour
{
    // ***** 인스펙터에 노출되는 필드 *****
    public RectTransform staffPanel;       // 오선 패널 전체 영역을 나타내는 RectTransform
    public RectTransform linesContainer;   // 오선(Line)들을 자식으로 보관할 컨테이너
    public GameObject linePrefab;          // 오선을 그리기 위해 인스턴스화할 프리팹
    public float staffHeight = 150f;       // 오선 전체 높이 (픽셀 단위)
    public float lineThickness = 7f;       // 오선의 두께 (픽셀 단위)

#if UNITY_EDITOR
    /// <summary>
    /// 인스펙터에서 값이 변경될 때 호출됨.
    /// 에디터 모드에서만 DelayedRedraw를 업데이트 루프에 등록하여
    /// 변경 사항을 반영한 후 오선을 다시 그리도록 함.
    /// </summary>
    private void OnValidate()
    {
        // 플레이 중이 아닐 때만 처리
        if (!Application.isPlaying)
        {
            // 중복 등록 방지: 기존 핸들러 제거
            EditorApplication.update -= DelayedRedraw;
            // 다음 업데이트 주기에 DelayedRedraw 호출 등록
            EditorApplication.update += DelayedRedraw;
        }
    }

    /// <summary>
    /// 에디터 업데이트 주기에 호출되어 실제로 오선을 재생성하는 메서드.
    /// OnValidate에서 등록된 후, 한 번 실행되면 바로 해제됨.
    /// </summary>
    private void DelayedRedraw()
    {
        // 다시 등록되지 않도록 핸들러 제거
        EditorApplication.update -= DelayedRedraw;

        // 필수 컴포넌트가 유효하지 않으면 중단
        if (!this || !linesContainer || !linePrefab) return;

        // 기존 오선 제거 후 새로 그리기
        ClearChildren();
        DrawStaffLines();
    }
#endif

    /// <summary>
    /// 플레이 모드 시작 시 한 번 실행되어 오선을 그리는 메서드.
    /// 에디터 모드에서도 ExecuteAlways 덕분에 Start가 호출될 수 있으나,
    /// 내부에서 Application.isPlaying으로 플레이 모드 여부를 체크함.
    /// </summary>
    private void Start()
    {
        // Staff_Panel을 화면 위쪽 절반에 배치
        if (staffPanel != null)
        {
            staffPanel.anchorMin = new Vector2(0, 0.5f);
            staffPanel.anchorMax = new Vector2(1, 1);
            staffPanel.pivot = new Vector2(0.5f, 1f);
            staffPanel.anchoredPosition = Vector2.zero;
            staffPanel.sizeDelta = Vector2.zero;
        }
        // staffHeight를 Staff_Panel 높이의 40%로 더 줄임
        staffHeight = staffPanel.rect.height * 0.4f;

        if (Application.isPlaying)
        {
            ClearChildren();
            DrawStaffLines();
        }
    }

    /// <summary>
    /// 오선(Staff Lines)을 실제로 생성하는 로직.
    /// 오선을 5줄로 고정하고, staffHeight와 lineThickness를 사용하여 배치함.
    /// </summary>
    private void DrawStaffLines()
    {
        int lineCount = 5;
        float spacing = staffHeight / (lineCount - 1);
        // 기준 Y 좌표: staffPanel의 아래에서 35% 위쪽
        float baseY = staffPanel.rect.height * 0.35f;

        for (int i = 0; i < lineCount; i++)
        {
            GameObject line = Instantiate(linePrefab, linesContainer);
            RectTransform rt = line.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, lineThickness);

            float rawY = baseY + staffHeight - i * spacing;
            rt.anchoredPosition = new Vector2(0, Mathf.Round(rawY));
        }
    }

    /// <summary>
    /// linesContainer 하위에 존재하는 모든 이전 오선 객체를 제거.
    /// linePrefab으로 생성된 객체만 이름 검사로 삭제함.
    /// </summary>
    private void ClearChildren()
    {
        // 역순으로 순회하여 안전하게 제거
        for (int i = linesContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = linesContainer.GetChild(i);
            // linePrefab 이름으로 시작하는 경우에만 삭제 대상
            if (child.name.StartsWith(linePrefab.name))
            {
#if UNITY_EDITOR
                // 에디터 모드: 즉시 삭제하여 씬 뷰 반영
                if (!Application.isPlaying)
                    Object.DestroyImmediate(child.gameObject);
                else
#endif
                // 플레이 모드: 일반 Destroy 호출
                Destroy(child.gameObject);
            }
        }
    }
}
