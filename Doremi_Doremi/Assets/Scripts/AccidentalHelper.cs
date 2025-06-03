using UnityEngine;

// AccidentalHelper.cs - 임시표 처리를 담당하는 헬퍼 클래스
public static class AccidentalHelper
{
    // 🎼 임시표 생성 함수
    public static float SpawnAccidental(Vector2 notePosition, AccidentalType accidental, float staffSpacing, 
        RectTransform staffPanel, GameObject sharpPrefab, GameObject flatPrefab, GameObject naturalPrefab, 
        GameObject doubleSharpPrefab, GameObject doubleFlatPrefab)
    {
        if (accidental == AccidentalType.None)
            return 0f;

        GameObject prefabToUse = null;
        float accidentalWidth = 0f;
        float accidentalHeight = 0f;

        switch (accidental)
        {
            case AccidentalType.Sharp:
                prefabToUse = sharpPrefab;
                accidentalWidth = staffSpacing * 0.8f;
                accidentalHeight = staffSpacing * 1.8f;
                break;
                
            case AccidentalType.Flat:
                prefabToUse = flatPrefab;
                accidentalWidth = staffSpacing * 0.8f;
                accidentalHeight = staffSpacing * 1.5f;
                break;
                
            case AccidentalType.Natural:
                prefabToUse = naturalPrefab;
                accidentalWidth = staffSpacing * 0.6f;
                accidentalHeight = staffSpacing * 2.2f;
                break;
                
            case AccidentalType.DoubleSharp:
                prefabToUse = doubleSharpPrefab;
                accidentalWidth = staffSpacing * 1.0f;
                accidentalHeight = staffSpacing * 1.0f;
                break;
                
            case AccidentalType.DoubleFlat:
                prefabToUse = doubleFlatPrefab;
                accidentalWidth = staffSpacing * 1.0f;
                accidentalHeight = staffSpacing * 1.5f;
                break;
        }

        if (prefabToUse == null)
        {
            Debug.LogWarning($"⚠️ {accidental} 임시표 프리팹이 설정되지 않았습니다.");
            return staffSpacing * 0.3f;
        }

        GameObject accidentalInstance = Object.Instantiate(prefabToUse, staffPanel);
        RectTransform accidentalRT = accidentalInstance.GetComponent<RectTransform>();

        // 🎯 해상도 독립적 크기 설정
        accidentalRT.sizeDelta = new Vector2(accidentalWidth, accidentalHeight);

        // 🎯 앵커와 피벗 설정
        accidentalRT.anchorMin = new Vector2(0.5f, 0.5f);
        accidentalRT.anchorMax = new Vector2(0.5f, 0.5f);
        accidentalRT.pivot = new Vector2(0.5f, 0.5f);

        // 🎯 위치 설정 (음표 바로 왼쪽)
        float accidentalX = notePosition.x - accidentalWidth * 1.2f; // 음표 왼쪽에 배치
        float accidentalY = notePosition.y;
        
        // 플랫 Y 오프셋 적용
        if (accidental == AccidentalType.Flat || accidental == AccidentalType.DoubleFlat)
        {
            accidentalY += staffSpacing * 0.1f;
        }

        accidentalRT.anchoredPosition = new Vector2(accidentalX, accidentalY);

        Debug.Log($"🎼 {accidental} 임시표: 크기={accidentalWidth:F1}x{accidentalHeight:F1}, 위치=({accidentalX:F1}, {accidentalY:F1})");

        return accidentalWidth; // 임시표가 차지하는 공간
    }
}