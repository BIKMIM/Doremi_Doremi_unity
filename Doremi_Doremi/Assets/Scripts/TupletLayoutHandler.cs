using UnityEngine;
using System.Collections.Generic;

// 잇단음표 레이아웃 계산 및 배치를 담당하는 클래스
public class TupletLayoutHandler : MonoBehaviour
{
    [Header("레이아웃 설정")]
    [Range(0.6f, 1.2f)]
    public float tupletWidthMultiplier = 0.8f; // 잇단음표 폭 배수

    [Range(0.8f, 2.0f)] 
    public float numberHeightOffset = 1.2f; // 숫자 높이 오프셋 (spacing 배수)

    [Range(2.0f, 4.0f)]
    public float beamHeightOffset = 2.8f; // beam 높이 오프셋 (spacing 배수)

    [Header("박자 설정")]
    [Range(1f, 4f)]
    public float defaultMeasureBeats = 2.0f; // 기본 마디 박자 수

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
        }
    }

    // 잇단음표 그룹의 전체 폭 계산
    public float CalculateTupletWidth(TupletData tupletData, float normalSpacing, float measureWidth, int totalNotesInMeasure)
    {
        if (!tupletData.IsComplete())
        {
            Debug.LogWarning("잇단음표 그룹이 완성되지 않았습니다.");
            return normalSpacing * tupletData.noteCount;
        }

        // 잇단음표가 차지해야 하는 시간 비율 계산
        float timeRatio = tupletData.GetTimeRatio(); // beatValue / noteCount
        
        // 4분음표 기준으로 실제 차지하는 박자 계산
        float actualBeats = tupletData.beatValue * 0.25f; // 2박자 = 0.5, 3박자 = 0.75
        
        // 마디 내에서 차지하는 폭 비율
        float widthRatio = actualBeats / GetMeasureBeats();
        float calculatedWidth = measureWidth * widthRatio;
        
        // 설정된 배수 적용
        calculatedWidth *= tupletWidthMultiplier;
        
        // 최소/최대 폭 제한
        float minWidth = normalSpacing * tupletData.noteCount * 0.5f; // 최소 50%
        float maxWidth = normalSpacing * tupletData.noteCount * 1.5f; // 최대 150%
        
        calculatedWidth = Mathf.Clamp(calculatedWidth, minWidth, maxWidth);
        
        if (showDebugInfo)
        {
            Debug.Log($"🎼 잇단음표 폭 계산: {tupletData.GetTupletTypeName()}");
            Debug.Log($"   시간비율: {timeRatio:F2}, 실제박자: {actualBeats:F2}");
            Debug.Log($"   폭배수: {tupletWidthMultiplier}, 계산폭: {calculatedWidth:F1}");
            Debug.Log($"   범위: {minWidth:F1}~{maxWidth:F1}");
        }
        
        return calculatedWidth;
    }

    // 잇단음표 내부 음표들의 위치 계산
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
        tupletData.noteSpacing = totalWidth / tupletData.noteCount;
        tupletData.centerX = startX + totalWidth * 0.5f;

        // 음표들의 Y 위치 범위 계산 (숫자 위치 결정용) - staffPanel 참조 전달
        tupletData.CalculateVerticalRange(staffPanel);

        if (showDebugInfo)
        {
            Debug.Log($"🎵 잇단음표 레이아웃: {tupletData.GetTupletTypeName()}");
            Debug.Log($"   시작X: {startX:F1}, 폭: {totalWidth:F1}, 간격: {tupletData.noteSpacing:F1}");
            Debug.Log($"   Y범위: {tupletData.minNoteY:F1} ~ {tupletData.maxNoteY:F1}");
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
        
        // beam의 시작과 끝 X 위치
        float startX = tupletData.startX + tupletData.noteSpacing * 0.3f; // 첫 번째 음표에서 약간 오른쪽
        float endX = tupletData.startX + tupletData.totalWidth - tupletData.noteSpacing * 0.3f; // 마지막 음표에서 약간 왼쪽
        
        Vector2 startPos = new Vector2(startX, beamY);
        Vector2 endPos = new Vector2(endX, beamY);
        float thickness = spacing * 0.15f; // beam 두께
        
        if (showDebugInfo)
        {
            Debug.Log($"🌉 beam 위치: ({startPos.x:F1}, {startPos.y:F1}) → ({endPos.x:F1}, {endPos.y:F1})");
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
        tupletWidthMultiplier = 0.8f;
        numberHeightOffset = 1.2f;
        beamHeightOffset = 2.8f;
        defaultMeasureBeats = 2.0f;
        
        Debug.Log("✅ TupletLayoutHandler 설정 리셋 완료");
    }

    [ContextMenu("현재 설정 출력")]
    public void PrintCurrentSettings()
    {
        Debug.Log("📊 === TupletLayoutHandler 현재 설정 ===");
        Debug.Log($"   잇단음표 폭 배수: {tupletWidthMultiplier}");
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