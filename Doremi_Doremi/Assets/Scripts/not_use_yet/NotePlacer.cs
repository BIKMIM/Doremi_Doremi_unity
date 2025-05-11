using UnityEngine;  // Unity 엔진의 핵심 기능을 제공하는 네임스페이스

[ExecuteAlways]  // 에디터 모드와 플레이 모드 모두에서 스크립트가 실행되도록 설정
public class NotePlacer : MonoBehaviour
{
    [Header("🎵 배치 대상")]  // 인스펙터에서 그룹화된 필드
    public RectTransform staffPanel;  // 오선 패널을 나타내는 RectTransform
    public RectTransform noteImage;   // 배치될 음표 이미지의 RectTransform
    public float staffHeight = 150f;  // 오선 전체 높이 (픽셀 단위)

    [Header("⚙ 설정: 음 높이")]
    [Range(-2f, 6f)]              // 인스펙터에서 조정 가능한 슬라이더 범위 설정: -2에서 6까지
    public float lineIndex = 0f;   // 오선 기준점으로부터의 음 높이 인덱스

    /// <summary>
    /// 게임 실행 시(또는 에디터 모드 Start 호출 시) 음표 배치 로직을 실행
    /// </summary>
    private void Start()
    {
        PlaceNote();  // 음표 위치 계산 및 배치 수행
    }

#if UNITY_EDITOR
    /// <summary>
    /// 인스펙터에서 값이 변경될 때마다 호출됨.
    /// 플레이 모드가 아닌 에디터 상태에서 Note 배치를 즉시 반영.
    /// </summary>
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            PlaceNote();  // 에디터에서 즉시 음표 위치 업데이트
        }
    }
#endif

    /// <summary>
    /// lineIndex 값을 기반으로 오선 위/아래에 음표 이미지를 배치하는 메서드
    /// </summary>
    public void PlaceNote()
    {
        // 필수 참조값이 없으면 로직 중단
        if (noteImage == null || staffPanel == null) return;

        // 오선 5줄(간격 4칸) 기준으로 공간 간격 계산
        float spacing = staffHeight / 4f;

        // 계산된 간격에 lineIndex 곱하여 Y 위치 산출
        float y = lineIndex * spacing;

        // 기존 X 좌표는 유지한 채 Y만 변경하여 위치 적용
        noteImage.anchoredPosition = new Vector2(noteImage.anchoredPosition.x, y);
    }
}