using UnityEngine;

//NotePlacer.cs의 역할
// “인스펙터에서 지정한 lineIndex 값에 따라,
// 하나의 음표 이미지를 오선지 위의
// 정확한 Y 위치로 이동시키는 것” 세부적으로 보면:
// lineIndex
// - 2에서 6까지 범위의 실수로,
// 이 값이 “오선지 위에서 몇 칸(또는 반칸) 위/아래”인지를 나타냄
// 예를 들어 0은 첫 번째 줄(E4), 0.5는 줄과 줄 사이, 1은 두 번째 줄(G4)을 의미함



// 에디터 모드와 플레이 모드 모두에서 스크립트가 실행되도록 하는 어트리뷰트
//[ExecuteAlways]
public class NotePlacer : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────────
    // 🎼 오선지(스태프) 전체를 담고 있는 RectTransform 참조
    public RectTransform staffPanel;

    // 📏 오선지 전체 높이(픽셀 단위) — 5줄 간격을 4칸으로 나눌 때 기준값
    public float staffHeight = 150f;

    // ⬆️ 음표 이미지가 줄 위/아래에 정확히 위치하도록 약간 보정하는 Y 오프셋
    public float noteYOffset = -10f;

    // 🔢 인스펙터에서 조정 가능한 라인 인덱스 (오선지 위에서 몇 칸/반칸 위/아래인지)
    [Range(-2f, 6f)] public float lineIndex = 0f;

    // ⚙️ 자기 자신(RectTransform)에 직접 접근하기 위한 캐시 변수
    private RectTransform rt;

    // Awake 단계에서 RectTransform 컴포넌트 캐시
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    // 게임이 시작될 때 한 번 실행하여 음표 위치 배치
    private void Start()
    {
        PlaceNote();
    }

#if UNITY_EDITOR
    // 에디터 인스펙터에서 값이 변경될 때마다 자동으로 위치 갱신
    private void OnValidate()
    {
        // 플레이 모드가 아닐 때만 실행
        if (!Application.isPlaying)
            PlaceNote();
    }
#endif

    // 📌 실제 음표 위치 계산 및 적용 메서드
    public void PlaceNote()
    {
        // 필수 참조가 없으면 동작 중지
        if (staffPanel == null || rt == null)
            return;

        // 1) 오선지 간격 계산: 총 4칸 = staffHeight / 4
        float spacing = staffHeight / 4f;

        // 2) 오선지 기준선 Y 좌표 가져오기(소수점 반올림)
        float baseY = Mathf.Round(staffPanel.anchoredPosition.y);

        // 3) lineIndex에 따른 Y 위치 계산
        //    baseY + (칸 인덱스 * 간격) + 보정값
        float y = Mathf.Round(baseY + lineIndex * spacing + noteYOffset);

        // 4) 최종 앵커 위치 설정 (X는 기존 유지)
        Vector2 pos = rt.anchoredPosition;
        rt.anchoredPosition = new Vector2(pos.x, y);
    }
}
