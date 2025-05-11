using UnityEngine;  // Unity 엔진의 기본 기능들 (게임 오브젝트, 컴포넌트 등)을 사용하기 위해 필요
#if UNITY_EDITOR
using UnityEditor;  // 에디터 전용 API (EditorApplication 등)를 사용하기 위해 필요
#endif

// 에디터와 플레이 모드 모두에서 스크립트가 실행되도록 보장하는 어트리뷰트
[ExecuteAlways]
public class StaffLineRenderer : MonoBehaviour
{
    // 🎼 오선지 전체를 담고 있는 RectTransform 참조
    public RectTransform staffPanel;
    // ➖ 오선(직선) 프리팹: 복제하여 5줄을 그리는 데 사용
    public GameObject linePrefab;
    // 📏 오선지 전체 높이(픽셀 단위) - 5줄 간격 계산 기준
    public float staffHeight = 150f;
    // 🖊 오선 두께(픽셀 단위)
    public float lineThickness = 7f;

#if UNITY_EDITOR
    // 에디터 모드에서 스태프를 다시 그려야 할 때를 표시하는 플래그
    private bool needsRedraw = false;

    // 인스펙터 값 변경 시 호출되는 콜백
    private void OnValidate()
    {
        // 플레이 중이 아닐 때만 리드로우 예약
        if (!UnityEngine.Application.isPlaying)
        {
            needsRedraw = true;
            // 중복 등록 방지를 위해 기존 델리게이트 제거 후 추가
            EditorApplication.update -= DelayedRedraw;
            EditorApplication.update += DelayedRedraw;
        }
    }

    // 실제 리드로우를 수행하는 메서드 (에디터 업데이트 루프에서 호출)
    private void DelayedRedraw()
    {
        // 호출 후 다시 등록 해제
        EditorApplication.update -= DelayedRedraw;

        // 컴포넌트나 참조가 없는 경우 조기 종료
        if (!this || !staffPanel || !linePrefab) return;

        // 기존에 그려진 오선들 제거
        ClearChildren();
        // 새로운 오선 5줄 그리기
        DrawStaffLines();
        needsRedraw = false;
    }
#endif

    // ------------------------------------------------------------------------------
    // 플레이 모드에서 처음 실행될 때 오선을 그리는 로직
    private void Start()
    {
        // 런타임일 경우에만 오선을 다시 그림 (에디터 중복 방지)
        if (UnityEngine.Application.isPlaying)
        {
            ClearChildren();    // 기존 오선 제거
            DrawStaffLines();   // 새 오선 그리기
        }
    }

    // 오선 5줄을 계산하여 Instantiate하는 메서드
    private void DrawStaffLines()
    {
        int lineCount = 5;  // 오선 5줄
        // 5줄은 4칸이므로 높이를 4로 나눠서 줄 간 간격 계산
        float spacing = staffHeight / (lineCount - 1);
        // 스태프 패널의 기준 Y 좌표(소수점 반올림)
        float baseY = Mathf.Round(staffPanel.anchoredPosition.y);

        // 각 줄마다 프리팹 복제 및 위치 설정
        for (int i = 0; i < lineCount; i++)
        {
            GameObject line = Instantiate(linePrefab, staffPanel);
            RectTransform rt = line.GetComponent<RectTransform>();

            // 앵커를 좌우 끝, 아래쪽에 설정하여 전체 너비로 늘리기
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            // 피벗을 하단 중앙으로 설정
            rt.pivot = new Vector2(0.5f, 0);
            // 두께만 설정, 너비는 stretch로 채워짐
            rt.sizeDelta = new Vector2(0, lineThickness);

            // 각 줄의 Y 좌표: baseY + (전체 높이 - i*간격)
            float rawY = baseY + staffHeight - i * spacing;
            // 앵커 위치에 반올림된 Y 적용
            rt.anchoredPosition = new Vector2(0, Mathf.Round(rawY));
        }
    }

    // 기존에 생성된 오선 프리팹만 찾아서 삭제하는 메서드
    private void ClearChildren()
    {
        // 자식 오브젝트를 뒤에서부터 순회
        for (int i = staffPanel.childCount - 1; i >= 0; i--)
        {
            Transform child = staffPanel.GetChild(i);
            // linePrefab 이름으로 시작하는 오브젝트만 삭제 대상으로 판별
            if (child.name.StartsWith(linePrefab.name))
            {
#if UNITY_EDITOR
                // 에디터 모드에서는 즉시 삭제하여 씬 뷰가 즉시 반영되게 함
                if (!UnityEngine.Application.isPlaying)
                    Object.DestroyImmediate(child.gameObject);
                else
#endif
                // 런타임에서는 일반 Destroy 사용
                Destroy(child.gameObject);
            }
        }
    }
}
