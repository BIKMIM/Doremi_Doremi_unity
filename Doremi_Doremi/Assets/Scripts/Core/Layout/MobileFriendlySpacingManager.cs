using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모바일 친화적인 마디선 기반 화면 분할 시스템
/// 마디선 개수에 따라 화면을 적절히 나누고 음표를 고르게 분산 배치
/// </summary>
public static class MobileFriendlySpacingManager
{
    /// <summary>
    /// 박자표에서 분자(박자 수) 추출
    /// </summary>
    public static int GetBeatsPerMeasure(string timeSignature)
    {
        if (string.IsNullOrEmpty(timeSignature) || !timeSignature.Contains("/"))
        {
            Debug.LogWarning($"⚠️ 잘못된 박자표: {timeSignature}, 기본값 4 사용");
            return 4;
        }

        string[] parts = timeSignature.Split('/');
        if (parts.Length == 2 && int.TryParse(parts[0], out int beats))
        {
            return beats;
        }

        Debug.LogWarning($"⚠️ 박자표 파싱 실패: {timeSignature}, 기본값 4 사용");
        return 4;
    }

    /// <summary>
    /// 박자표 문자열 파싱
    /// </summary>
    public static (int beatsPerMeasure, int beatNote) ParseTimeSignature(string timeSignature)
    {
        if (string.IsNullOrEmpty(timeSignature) || !timeSignature.Contains("/"))
        {
            return (4, 4);
        }

        string[] parts = timeSignature.Split('/');
        if (parts.Length == 2 && 
            int.TryParse(parts[0], out int numerator) && 
            int.TryParse(parts[1], out int denominator))
        {
            return (numerator, denominator);
        }

        return (4, 4);
    }

    /// <summary>
    /// 🎯 마디선 개수에 따른 화면 분할 계산
    /// </summary>
    /// <param name="barLineCount">마디선 개수 (1개, 2개만 처리)</param>
    /// <param name="screenWidth">전체 화면 폭</param>
    /// <param name="usableRatio">사용 가능한 화면 비율 (0.8 = 80%)</param>
    /// <returns>(마디 개수, 마디당 폭)</returns>
    public static (int measureCount, float measureWidth) CalculateScreenDivision(
        int barLineCount, float screenWidth, float usableRatio = 0.9f)
    {
        // 사용 가능한 화면 폭
        float usableWidth = screenWidth * usableRatio;
        
        int measureCount;
        
        if (barLineCount == 1)
        {
            // 마디선 1개 = 화면 전체를 1마디로 사용
            measureCount = 1;
        }
        else if (barLineCount == 2)
        {
            // 마디선 2개 = 화면을 2마디로 나눔
            measureCount = 2;
        }
        else
        {
            // 3개 이상은 무시하고 기본값 (2마디)
            measureCount = 2;
            Debug.LogWarning($"⚠️ 마디선 {barLineCount}개는 지원하지 않습니다. 기본값(2마디) 사용");
        }
        
        float measureWidth = usableWidth / measureCount;
        
        Debug.Log($"📏 화면 분할: 마디선 {barLineCount}개 → {measureCount}마디, 각 마디 폭 {measureWidth:F1}px");
        
        return (measureCount, measureWidth);
    }

    /// <summary>
    /// 🎯 마디 내 음표들을 박자에 맞춰 고르게 분산 배치
    /// </summary>
    /// <param name="notes">마디 내 음표 리스트</param>
    /// <param name="timeSignature">박자표</param>
    /// <param name="measureStartX">마디 시작 X 위치</param>
    /// <param name="measureWidth">마디 폭</param>
    /// <param name="paddingRatio">마디 내부 여백 비율</param>
    /// <returns>각 음표의 X 위치 배열</returns>
    public static float[] CalculateEvenlyDistributedPositions(List<NoteData> notes, string timeSignature, 
        float measureStartX, float measureWidth, float paddingRatio = 0.1f)
    {
        if (notes == null || notes.Count == 0)
            return new float[0];

        // 마디 내부 여백 적용
        float padding = measureWidth * paddingRatio;
        float usableWidth = measureWidth - (padding * 2f);
        float contentStartX = measureStartX + padding;

        var (beatsPerMeasure, beatNote) = ParseTimeSignature(timeSignature);
        
        float[] positions = new float[notes.Count];
        
        if (notes.Count == 1)
        {
            // 음표가 1개면 마디 중앙에 배치
            positions[0] = contentStartX + (usableWidth * 0.5f);
            Debug.Log($"   단일음표: 마디 중앙 배치 X={positions[0]:F1}");
        }
        else
        {
            // 음표가 여러 개면 고르게 분산 배치
            for (int i = 0; i < notes.Count; i++)
            {
                // 0부터 1까지의 비율로 각 음표 위치 계산
                float ratio = (float)i / (notes.Count - 1);
                positions[i] = contentStartX + (usableWidth * ratio);
                
                Debug.Log($"   음표 {i}: {notes[i].noteName} → 비율 {ratio:F2}, X={positions[i]:F1}");
            }
        }

        return positions;
    }

    /// <summary>
    /// 🎯 잇단음표 그룹의 위치 계산 (고르게 분산)
    /// </summary>
    /// <param name="tuplet">잇단음표 데이터</param>
    /// <param name="allElementsCount">마디 내 전체 요소 개수</param>
    /// <param name="currentIndex">현재 잇단음표의 인덱스</param>
    /// <param name="measureStartX">마디 시작 X</param>
    /// <param name="measureWidth">마디 폭</param>
    /// <param name="paddingRatio">여백 비율</param>
    /// <returns>(시작X, 폭)</returns>
    public static (float startX, float width) CalculateTupletPosition(
        TupletData tuplet, int allElementsCount, int currentIndex,
        float measureStartX, float measureWidth, float paddingRatio = 0.1f)
    {
        // 마디 내부 여백 적용
        float padding = measureWidth * paddingRatio;
        float usableWidth = measureWidth - (padding * 2f);
        float contentStartX = measureStartX + padding;

        if (allElementsCount == 1)
        {
            // 잇단음표만 있으면 마디 중앙에 배치
            float centerX = contentStartX + (usableWidth * 0.5f);
            float tupletWidth = usableWidth * 0.6f; // 마디 폭의 60%
            float startX = centerX - (tupletWidth * 0.5f);
            
            Debug.Log($"   단일 잇단음표: 마디 중앙 배치 X={startX:F1}, 폭={tupletWidth:F1}");
            return (startX, tupletWidth);
        }
        else
        {
            // 여러 요소가 있으면 고르게 분산 배치
            float ratio = (float)currentIndex / (allElementsCount - 1);
            float elementSpacing = usableWidth / allElementsCount;
            
            float startX = contentStartX + (ratio * usableWidth) - (elementSpacing * 0.3f);
            float width = elementSpacing * 0.6f; // 각 요소 공간의 60%
            
            Debug.Log($"   잇단음표 {currentIndex}: 비율 {ratio:F2}, X={startX:F1}, 폭={width:F1}");
            return (startX, width);
        }
    }

    /// <summary>
    /// 음표의 박자 값 계산
    /// </summary>
    public static float GetNoteBeatValue(int noteValue, bool isDotted, int timeSignatureDenominator = 4)
    {
        float beatValue = (float)timeSignatureDenominator / noteValue;
        
        if (isDotted)
        {
            beatValue *= 1.5f;
        }
        
        return beatValue;
    }

    /// <summary>
    /// 디버그: 화면 분할 정보 출력
    /// </summary>
    public static void DebugScreenDivision(int barLineCount, string timeSignature, float screenWidth)
    {
        var (measureCount, measureWidth) = CalculateScreenDivision(barLineCount, screenWidth);
        int beatsPerMeasure = GetBeatsPerMeasure(timeSignature);
        
        Debug.Log($"📱 모바일 친화적 화면 분할 ({timeSignature}):");
        Debug.Log($"   마디선 개수: {barLineCount}");
        Debug.Log($"   화면 분할: {measureCount}마디");
        Debug.Log($"   각 마디 폭: {measureWidth:F1}px");
        Debug.Log($"   박자당 공간: {measureWidth / beatsPerMeasure:F1}px");
        
        for (int i = 0; i < measureCount; i++)
        {
            float measureStartX = i * measureWidth;
            Debug.Log($"   마디 {i + 1}: X={measureStartX:F1} ~ {measureStartX + measureWidth:F1}");
        }
    }
}
