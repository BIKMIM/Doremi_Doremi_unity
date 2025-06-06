using UnityEngine;
using System.Collections.Generic;

// 잇단음표 레이아웃 계산 및 배치를 담당하는 클래스 - 비율 기반 간격 지원
public class TupletLayoutHandler : MonoBehaviour
{
    [Header("🎯 비율 기반 레이아웃 설정")]
    [Range(0.5f, 1.5f)]
    public float tupletCompressionRatio = 0.7f; // 잇단음표 압축 비율 (70%)

    [Header("레이아웃 설정")]
    [Range(0.8f, 2.0f)] 
    public float numberHeightOffset = 1.2f; // 숫자 높이 오프셋 (spacing 배수)

    [Range(2.0f, 4.0f)]
    public float beamHeightOffset = 2.8f; // beam 높이 오프셋 (spacing 배수)

    [Header("음표 간격 세부 조정")]
    [Range(0.5f, 2.0f)]
    public float noteSpacingMultiplier = 1.0f; // 잇단음표 내부 음표 간격 배수

    [Header("디버그 정보")]
    public bool showDebugInfo = true;
    public bool showLayoutBounds = false; // 레이아웃 경계 표시 (나중에 Gizmo로 구현)

    private RectTransform staffPanel;

    public void Initialize(RectTransform panel)
    {
        staffPanel = panel;
        
        if (showDebugInfo)
        {
            Debug.Log($"✅ TupletLayoutHandler 초기화 완료 (비율 기반)");
            Debug.Log($"   압축 비율: {tupletCompressionRatio}");
            Debug.Log($"   숫자 높이 오프셋: {numberHeightOffset}");
            Debug.Log($"   beam 높이 오프셋: {beamHeightOffset}");
            Debug.Log($"   음표 간격 배수: {noteSpacingMultiplier}");
        }
    }

    // ✅ NEW: 비율 기반 잇단음표 폭 계산 (NoteSpawner에서 호출됨)
    public float CalculateTupletWidth(TupletData tupletData, float normalSpacing, float baseWidth, int totalNotesInMeasure)
    {
        if (!tupletData.IsComplete())
        {
            Debug.LogWarning("잇단음표 그룹이 완성되지 않았습니다.");
            return normalSpacing * tupletData.noteCount;
        }

        // 🎯 비율 기반 압축 적용
        float compressedWidth = baseWidth * tupletCompressionRatio;
        
        // 최소 폭 보장 (음표들이 너무 겹치지 않게)
        float minWidth = normalSpacing * tupletData.noteCount * 0.8f;
        compressedWidth = Mathf.Max(compressedWidth, minWidth);
        
        if (showDebugInfo)
        {
            Debug.Log($"🎼 비율 기반 잇단음표 폭: {tupletData.GetTupletTypeName()}");
            Debug.Log($"   기본폭: {baseWidth:F1}, 압축비율: {tupletCompressionRatio:F1}");
            Debug.Log($"   압축폭: {compressedWidth:F1}, 최소폭: {minWidth:F1}");
        }
        
        return compressedWidth;
    }

    // ✅ 개선된 음표 내부 배치 (해상도 독립적)
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
        
        // 🎯 해상도 독립적 여백 계산
        float marginRatio = 0.1f; // 10% 여백 (비율 기반)
        float usableWidth = totalWidth * (1f - marginRatio * 2f);
        float leftMargin = totalWidth * marginRatio;
        
        // 음표 간격 계산 (균등 배치)
        if (tupletData.noteCount > 1)
        {
            tupletData.noteSpacing = (usableWidth / (tupletData.noteCount - 1)) * noteSpacingMultiplier;
        }
        else
        {
            tupletData.noteSpacing = usableWidth * noteSpacingMultiplier;
        }
        
        // 중앙 X 좌표 (숫자 배치용)
        tupletData.centerX = startX + totalWidth * 0.5f;

        // 음표들의 Y 위치 범위 계산 (숫자 위치 결정용)
        tupletData.CalculateVerticalRange(staffPanel);

        if (showDebugInfo)
        {
            Debug.Log($"🎵 잇단음표 레이아웃 (비율 기반): {tupletData.GetTupletTypeName()}");
            Debug.Log($"   시작X: {startX:F1}, 총폭: {totalWidth:F1}");
            Debug.Log($"   여백비율: {marginRatio:P0}, 사용가능폭: {usableWidth:F1}");
            Debug.Log($"   음표간격: {tupletData.noteSpacing:F1} (배수: {noteSpacingMultiplier:F1})");
            Debug.Log($"   중앙X: {tupletData.centerX:F1}");
        }
    }

    // 잇단음표 숫자 위치 계산 (해상도 독립적)
    public Vector2 CalculateTupletNumberPosition(TupletData tupletData, float spacing)
    {
        if (!tupletData.IsComplete())
        {
            return Vector2.zero;
        }

        float x = tupletData.centerX;
        
        // 설정된 높이 오프셋 사용 (spacing 기반으로 해상도 독립적)
        float y = tupletData.maxNoteY + spacing * numberHeightOffset;
        
        // 최소 높이 보장 (stem이 위로 향하는 경우 고려)
        float minY = spacing * 2.0f;
        y = Mathf.Max(y, minY);

        Vector2 position = new Vector2(x, y);
        
        if (showDebugInfo)
        {
            Debug.Log($"🔢 잇단음표 숫자 위치: ({x:F1}, {y:F1}), 오프셋배수: {numberHeightOffset}");
        }
        
        return position;
    }

    // beam(연결선) 위치 계산 (해상도 독립적)
    public (Vector2 startPos, Vector2 endPos, float thickness) CalculateBeamPositions(TupletData tupletData, float spacing, List<GameObject> stemObjects)
    {
        if (!tupletData.IsComplete() || stemObjects.Count != tupletData.noteCount)
        {
            Debug.LogWarning("beam 위치 계산 실패: 데이터 불일치");
            return (Vector2.zero, Vector2.zero, 0f);
        }

        // beam의 Y 위치 결정 (모든 stem의 끝점 기준)
        float beamY = CalculateBeamY(tupletData, stemObjects, spacing);
        
        // 🎯 비율 기반 beam X 위치 (해상도 독립적)
        float marginRatio = 0.1f; // 10% 여백
        float leftMargin = tupletData.totalWidth * marginRatio;
        float rightMargin = tupletData.totalWidth * marginRatio;
        
        float startX = tupletData.startX + leftMargin;
        float endX = tupletData.startX + tupletData.totalWidth - rightMargin;
        
        Vector2 startPos = new Vector2(startX, beamY);
        Vector2 endPos = new Vector2(endX, beamY);
        
        // beam 두께도 spacing 기반으로 해상도 독립적
        float thickness = spacing * 0.15f; 

        if (showDebugInfo)
        {
            Debug.Log($"🌉 beam 위치 (비율 기반):");
            Debug.Log($"   startX={startX:F1}, endX={endX:F1}, 여백비율={marginRatio:P0}");
            Debug.Log($"   startPos=({startPos.x:F1}, {startPos.y:F1}), endPos=({endPos.x:F1}, {endPos.y:F1})");
            Debug.Log($"   두께: {thickness:F2}, Y오프셋배수: {beamHeightOffset}");
        }

        return (startPos, endPos, thickness);
    }

    // beam의 Y 위치 계산 (stem 방향 고려, 해상도 독립적)
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
            // stem이 아래로: beam을 아래쪽에 배치 (spacing 기반)
            beamY = tupletData.minNoteY - spacing * beamHeightOffset;
        }
        else
        {
            // stem이 위로: beam을 위쪽에 배치 (spacing 기반)
            beamY = tupletData.maxNoteY + spacing * beamHeightOffset;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"🎯 beam Y 계산: 평균음표인덱스={avgNoteIndex:F1}, stem아래={stemDown}");
            Debug.Log($"   beamY={beamY:F1}, spacing={spacing:F1}, 오프셋배수={beamHeightOffset}");
        }
        
        return beamY;
    }

    // ✅ 비율 기반 압축 적용 메서드들 (NoteSpawner에서 호출)
    public void SetCompressionRatio(float ratio)
    {
        tupletCompressionRatio = Mathf.Clamp(ratio, 0.5f, 1.5f);
        if (showDebugInfo)
        {
            Debug.Log($"🎯 잇단음표 압축비율 설정: {tupletCompressionRatio:F1}");
        }
    }

    public float GetCompressionRatio()
    {
        return tupletCompressionRatio;
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

    // ✅ 설정값 런타임 조정 (비율 기반 테스트용)
    [ContextMenu("🎼 압축 증가 (더 좁게)")]
    public void IncreaseCompression()
    {
        tupletCompressionRatio = Mathf.Max(tupletCompressionRatio - 0.1f, 0.5f);
        Debug.Log($"🎼 잇단음표 압축비율: {tupletCompressionRatio:F1} (더 좁게)");
    }

    [ContextMenu("🎼 압축 감소 (더 넓게)")]
    public void DecreaseCompression()
    {
        tupletCompressionRatio = Mathf.Min(tupletCompressionRatio + 0.1f, 1.5f);
        Debug.Log($"🎼 잇단음표 압축비율: {tupletCompressionRatio:F1} (더 넓게)");
    }

    [ContextMenu("🔄 설정 리셋")]
    public void ResetSettings()
    {
        tupletCompressionRatio = 0.7f; // 70%
        noteSpacingMultiplier = 1.0f;
        numberHeightOffset = 1.2f;
        beamHeightOffset = 2.8f;
        
        Debug.Log("✅ TupletLayoutHandler 설정 리셋 완료 (비율 기반)");
    }

    [ContextMenu("📊 현재 설정 출력")]
    public void PrintCurrentSettings()
    {
        Debug.Log("📊 === TupletLayoutHandler 현재 설정 (비율 기반) ===");
        Debug.Log($"   잇단음표 압축비율: {tupletCompressionRatio:F1} (낮을수록 좁게)");
        Debug.Log($"   음표 간격 배수: {noteSpacingMultiplier:F1}");
        Debug.Log($"   숫자 높이 오프셋: {numberHeightOffset:F1}");
        Debug.Log($"   beam 높이 오프셋: {beamHeightOffset:F1}");
        Debug.Log($"   디버그 모드: {showDebugInfo}");
    }

    // staffPanel 참조 반환 (다른 클래스에서 필요할 경우)
    public RectTransform GetStaffPanel()
    {
        return staffPanel;
    }
}
