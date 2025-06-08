using UnityEngine;

/// <summary>
/// 점음표(dot) 생성 전용 컴포넌트
/// </summary>
public class NoteDotCreator : MonoBehaviour
{
    [Header("Dot 프리팹")]
    public GameObject dotPrefab;

    /// <summary>
    /// 음표에 점 붙이기
    /// </summary>
    public GameObject AttachDot(GameObject headOrRest, bool isOnLine, RectTransform parent)
    {
        if (dotPrefab == null || headOrRest == null)
        {
            Debug.LogError("❌ Dot 생성 실패: dotPrefab 또는 headOrRest가 null");
            return null;
        }

        float spacing = MusicLayoutConfig.GetSpacing(parent);
        float dotSize = spacing * 0.35f;
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;

        GameObject dot = Instantiate(dotPrefab, headOrRest.transform);
        RectTransform rt = dot.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);

        // 점 위치 계산
        float x = headWidth * 0.6f + spacing * 0.2f;
        float y = isOnLine ? spacing * 0.25f : 0f; // 줄 위 음표는 위로 이동

        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(dotSize, dotSize);
        rt.localScale = Vector3.one;

        Debug.Log($"🎯 음표 점 추가: 위치=({x:F1}, {y:F1}), 줄위음표={isOnLine}");
        return dot;
    }

    /// <summary>
    /// 쉼표에 점 붙이기 (쉼표별 맞춤형 위치)
    /// </summary>
    public GameObject AttachRestDot(GameObject rest, int duration, RectTransform parent)
    {
        if (dotPrefab == null || rest == null)
        {
            Debug.LogError("❌ Rest Dot 생성 실패: dotPrefab 또는 rest가 null");
            return null;
        }

        float spacing = MusicLayoutConfig.GetSpacing(parent);
        float dotSize = spacing * 0.3f;
        
        GameObject dot = Instantiate(dotPrefab, rest.transform);
        RectTransform rt = dot.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // 쉼표별 맞춤형 점 위치
        Vector2 dotOffset = GetRestDotOffset(duration, spacing);
        rt.anchoredPosition = dotOffset;
        rt.sizeDelta = new Vector2(dotSize, dotSize);
        rt.localScale = Vector3.one;

        Debug.Log($"🎯 쉼표 점 추가: {duration}분쉼표, 위치: {dotOffset}");
        return dot;
    }

    /// <summary>
    /// 쉼표별 점 위치 계산
    /// </summary>
    private Vector2 GetRestDotOffset(int duration, float spacing)
    {
        return duration switch
        {
            1 => new Vector2(spacing * 0.8f, spacing * 0.2f),
            2 => new Vector2(spacing * 0.8f, spacing * 0.0f),
            4 => new Vector2(spacing * 0.6f, spacing * 1.0f),
            8 => new Vector2(spacing * 0.6f, spacing * 0.8f),
            16 => new Vector2(spacing * 0.6f, spacing * 0.4f),
            _ => new Vector2(spacing * 0.8f, spacing * 0.2f)
        };
    }

    /// <summary>
    /// 프리팹 유효성 검사
    /// </summary>
    public bool ValidatePrefab()
    {
        if (dotPrefab == null)
        {
            Debug.LogWarning("⚠️ dotPrefab이 없습니다");
            return false;
        }
        return true;
    }
}
