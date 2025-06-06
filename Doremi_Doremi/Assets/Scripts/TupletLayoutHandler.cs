using UnityEngine;
using System.Collections.Generic;

// 잇단음표 레이아웃 계산 및 배치를 담당하는 클래스
public class TupletLayoutHandler : MonoBehaviour
{
    [Header("레이아웃 설정")]
    [Range(0.6f, 2.0f)]
    public float tupletWidthMultiplier = 1.4f; // 잇단음표 폭 배수 (증가됨)

    [Range(0.8f, 2.0f)] 
    public float numberHeightOffset = 1.2f; // 숫자 높이 오프셋 (spacing 배수)

    [Range(2.0f, 4.0f)]
    public float beamHeightOffset = 2.8f; // beam 높이 오프셋 (spacing 배수)

    [Header("박자 설정")]
    [Range(1f, 8f)]
    public float defaultMeasureBeats = 3.0f; // 기본 마디 박자 수 (3박자로 설정)

    [Header("음표 간격 세부 조정")]
    [Range(0.5f, 2.0f)]
    public float noteSpacingMultiplier = 1.2f; // 잇단음표 내부 음표 간격 추가 배수

    [Header("디버그 정보")]
    public bool showDebugInfo = true;
    public bool showLayoutBounds = false; // 레이아웃 경계 표시 (나중에 Gizmo로 구현)

    private RectTransform staffPanel;

    public void Initialize(RectTransform panel)
    {
        staffPanel = panel;
        
        if (showDebugInfo)
        {
            Debug.Log($"✅ TupletLayoutHandler 초기화 완료");
            Debug.Log($"   폭 배수: {tupletWidthMultiplier}");
            Debug.Log($"   숫자 높이 오프셋: {numberHeightOffset}");
            Debug.Log($"   beam 높이 오프셋: {beamHeightOffset}");
            Debug.Log($"   마디 박자 수: {defaultMeasureBeats}");
            Debug.Log($"   음표 간격 배수: {noteSpacingMultiplier}");
        }
    }

    // 잇단음표 그룹의 전체 폭 계산 (박자 기반 개선)
    public float CalculateTupletWidth(TupletData tupletData, float normalSpacing, float measureWidth, int totalNotesInMeasure)
    {
        if (!tupletData.IsComplete())
        {
            Debug.LogWarning("잇단음표 그룹이 완성되지 않았습니다.");
            return normalSpacing * tupletData.noteCount;
        }

        // 🎯 박자 기반 공간 배분 계산
        float timeRatio = tupletData.GetTimeRatio(); // beatValue / noteCount
        float actualBeats = tupletData.beatValue * 0.25f; // 2박자 = 0.5, 3박자 = 0.75
        
        // 마디 내에서 이 잇단음표가 차지해야 하는 공간 비율
        float beatRatio = actualBeats / GetMeasureBeats();
        
        // 전체 마디 폭에서 박자 비율만큼 할당
        float calculatedWidth = measureWidth * beatRatio;
        
        // 설정된 배수 적용
        calculatedWidth *= tupletWidthMultiplier;
        
        // 잇단음표 내부 음표 간격을 더 넓게
        float minWidthForNotes = normalSpacing * tupletData.noteCount * noteSpacingMultiplier;
        calculatedWidth = Mathf.Max(calculatedWidth, minWidthForNotes);
        
        // 최대 폭 제한 (마디를 넘지 않게)
        float maxWidth = measureWidth * 0.8f; // 마디의 80%까지만
        calculatedWidth = Mathf.Min(calculatedWidth, maxWidth);
        
        if (showDebugInfo)
        {
            Debug.Log($"🎼 잇단음표 폭 계산: {tupletData.GetTupletTypeName()}");
            Debug.Log($"   실제박자: {actualBeats:F2}, 마디박자: {GetMeasureBeats():F1}");
            Debug.Log($"   박자비율: {beatRatio:F2}, 폭배수: {tupletWidthMultiplier}");
            Debug.Log($"   계산폭: {calculatedWidth:F1}, 최소폭: {minWidthForNotes:F1}");
            Debug.Log($"   마디폭: {measureWidth:F1}, 최대폭: {maxWidth:F1}");
        }
        
        return calculatedWidth;
    }

    // 잇단음표 내부 음표들의 위치 계산 (간격 개선)
    public void LayoutTupletNotes(TupletData tupletData, float startX, float totalWidth, float spacing)
    {
        if (!tupletData.IsComplete())
        {
            Debug.LogWarning("잇단음표 그룹이 완성되지 않았습니다.");
            return;
        }

        // 기본 정보 설정
        tupletData.startX = startX;
        tupletData.totalWidth = totalWidth;
        
        // 🎯 개선된 음표 간격 계산
        // 양쪽 여백을 고려한 실제 음표 배치 공간
        float marginRatio = 0.1f; // 10% 여백
        float usableWidth = totalWidth * (1f - marginRatio * 2f);
        float leftMargin = totalWidth * marginRatio;
        
        // 음표 간격 (첫 음표와 마지막 음표는 여백 안쪽에 배치)
        if (tupletData.noteCount > 1)
        {
            tupletData.noteSpacing = usableWidth / (tupletData.noteCount - 1);
        }
        else
        {
            tupletData.noteSpacing = usableWidth;
        }
        
        // 중앙 X 좌표 (숫자 배치용)
        tupletData.centerX = startX + totalWidth * 0.5f;

        // 음표들의 Y 위치 범위 계산 (숫자 위치 결정용) - staffPanel 참조 전달
        tupletData.CalculateVerticalRange(staffPanel);

        if (showDebugInfo)
        {
            Debug.Log($"🎵 잇단음표 레이아웃: {tupletData.GetTupletTypeName()}");
            Debug.Log($"   시작X: {startX:F1}, 총폭: {totalWidth:F1}");
            Debug.Log($"   사용가능폭: {usableWidth:F1}, 음표간격: {tupletData.noteSpacing:F1}");
            Debug.Log($"   중앙X: {tupletData.centerX:F1}, Y범위: {tupletData.minNoteY:F1}~{tupletData.maxNoteY:F1}");
        }
    }

    // 잇단음표 숫자 위치 계산
    public Vector2 CalculateTupletNumberPosition(TupletData tupletData, float spacing)
    {
        if (!tupletData.IsComplete())
        {
            return Vector2.zero;
        }

        float x = tupletData.centerX;
        
        // 설정된 높이 오프셋 사용
        float y = tupletData.maxNoteY + spacing * numberHeightOffset;
        
        // 최소 높이 보장 (stem이 위로 향하는 경우 고려)
        float minY = spacing * 2.0f;
        y = Mathf.Max(y, minY);

        Vector2 position = new Vector2(x, y);
        
        if (showDebugInfo)
        {
            Debug.Log($"🔢 잇단음표 숫자 위치: ({x:F1}, {y:F1}), 오프셋: {numberHeightOffset}");
        }
        
        return position;
    }

    // beam(연결선) 위치 계산
    public (Vector2 startPos, Vector2 endPos, float thickness) CalculateBeamPositions(TupletData tupletData, float spacing, List<GameObject> stemObjects)
    {
        if (!tupletData.IsComplete() || stemObjects.Count != tupletData.noteCount)
        {
            Debug.LogWarning("beam 위치 계산 실패: 데이터 불일치");
            return (Vector2.zero, Vector2.zero, 0f);
        }

        // beam의 Y 위치 결정 (모든 stem의 끝점 기준)
        float beamY = CalculateBeamY(tupletData, stemObjects, spacing);
        
        // 🎯 개선된 beam X 위치 (음표 배치 고려)
        float marginRatio = 0.1f;
        float leftMargin = tupletData.totalWidth * marginRatio;
        float rightMargin = tupletData.totalWidth * marginRatio;
        
        float startX = tupletData.startX + leftMargin;
        float endX = tupletData.startX + tupletData.totalWidth - rightMargin;
        
        Vector2 startPos = new Vector2(startX, beamY);
        Vector2 endPos = new Vector2(endX, beamY);
        float thickness = spacing * 0.5f; // beam 두께

        if (showDebugInfo)
        {
            Debug.Log($"🌉 TLH: 개선된 beam 위치");
            Debug.Log($"   startX={startX:F1}, endX={endX:F1}");
            Debug.Log($"   startPos=({startPos.x:F1}, {startPos.y:F1}), endPos=({endPos.x:F1}, {endPos.y:F1})");
            Debug.Log($"   두께: {thickness:F2}, Y오프셋: {beamHeightOffset}");
        }

        return (startPos, endPos, thickness);
    }

    // beam의 Y 위치 계산 (stem 방향 고려)
    private float CalculateBeamY(TupletData tupletData, List<GameObject> stemObjects, float spacing)
    {
        // 평균 음표 높이로 stem 방향 결정
        float avgNoteIndex = 0f;
        int validNotes = 0;
        
        foreach (var note in tupletData.notes)
        {
            if (!note.isRest && NotePositioningData.noteIndexTable.ContainsKey(note.noteName))
            {
                avgNoteIndex += NotePositioningData.noteIndexTable[note.noteName];
                validNotes++;
            }
        }
        
        if (validNotes > 0)
        {
            avgNoteIndex /= validNotes;
        }
        
        // B4(0) 이상이면 stem이 아래로 향함
        bool stemDown = avgNoteIndex >= 0f;
        
        float beamY;
        if (stemDown)
        {
            // stem이 아래로: beam을 아래쪽에 배치
            beamY = tupletData.minNoteY - spacing * beamHeightOffset;
        }
        else
        {
            // stem이 위로: beam을 위쪽에 배치  
            beamY = tupletData.maxNoteY + spacing * beamHeightOffset;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"🎯 beam Y 계산: 평균음표인덱스={avgNoteIndex:F1}, stem아래={stemDown}");
            Debug.Log($"   beamY={beamY:F1}, 오프셋={beamHeightOffset}");
        }
        
        return beamY;
    }

    // 박자 기반 마디 공간 배분 계산
    public float CalculateBeatBasedSpacing(float totalBeats, float measureWidth)
    {
        return measureWidth / totalBeats;
    }

    // 특정 박자값에 대한 공간 계산
    public float CalculateSpaceForBeats(float beats, float beatSpacing)
    {
        return beats * beatSpacing;
    }

    // 현재 박자표의 한 마디 박자 수 반환
    private float GetMeasureBeats()
    {
        // 나중에 TimeSignature에서 가져오도록 개선 예정
        return defaultMeasureBeats;
    }

    // 잇단음표가 마디를 넘어가는지 확인
    public bool CheckTupletFitsInMeasure(TupletData tupletData, float availableWidth)
    {
        float requiredWidth = tupletData.totalWidth;
        bool fits = requiredWidth <= availableWidth;
        
        if (!fits && showDebugInfo)
        {
            Debug.LogWarning($"⚠️ 잇단음표가 마디를 초과: 필요={requiredWidth:F1}, 가능={availableWidth:F1}");
        }
        
        return fits;
    }

    // 잇단음표 그룹의 시각적 경계 계산 (디버그용)
    public Rect GetTupletBounds(TupletData tupletData, float spacing)
    {
        if (!tupletData.IsComplete())
        {
            return new Rect(0, 0, 0, 0);
        }

        float left = tupletData.startX;
        float right = tupletData.startX + tupletData.totalWidth;
        float bottom = tupletData.minNoteY - spacing * 1.0f;
        float top = tupletData.maxNoteY + spacing * (numberHeightOffset + 0.5f);
        
        return new Rect(left, bottom, right - left, top - bottom);
    }

    // 설정값 런타임 조정 (테스트용)
    [ContextMenu("설정 리셋")]
    public void ResetSettings()
    {
        tupletWidthMultiplier = 1.4f;
        noteSpacingMultiplier = 1.2f;
        numberHeightOffset = 1.2f;
        beamHeightOffset = 2.8f;
        defaultMeasureBeats = 3.0f;
        
        Debug.Log("✅ TupletLayoutHandler 설정 리셋 완료 (개선된 버전)");
    }

    [ContextMenu("현재 설정 출력")]
    public void PrintCurrentSettings()
    {
        Debug.Log("📊 === TupletLayoutHandler 현재 설정 (개선됨) ===");
        Debug.Log($"   잇단음표 폭 배수: {tupletWidthMultiplier}");
        Debug.Log($"   음표 간격 배수: {noteSpacingMultiplier}");
        Debug.Log($"   숫자 높이 오프셋: {numberHeightOffset}");
        Debug.Log($"   beam 높이 오프셋: {beamHeightOffset}");
        Debug.Log($"   기본 마디 박자: {defaultMeasureBeats}");
        Debug.Log($"   디버그 모드: {showDebugInfo}");
    }

    // staffPanel 참조 반환 (다른 클래스에서 필요할 경우)
    public RectTransform GetStaffPanel()
    {
        return staffPanel;
    }
}