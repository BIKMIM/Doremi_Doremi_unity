using UnityEngine;

/// <summary>
/// 쉼표 생성 전용 컴포넌트
/// </summary>
public class RestNoteCreator : MonoBehaviour
{
    [Header("쉼표 프리팹")]
    public GameObject rest1Prefab;  // 1분 쉼표
    public GameObject rest2Prefab;  // 2분 쉼표
    public GameObject rest4Prefab;  // 4분 쉼표
    public GameObject rest8Prefab;  // 8분 쉼표
    public GameObject rest16Prefab; // 16분 쉼표

    /// <summary>
    /// 쉼표 생성
    /// </summary>
    public GameObject CreateRestNote(Vector2 position, int duration, RectTransform parent)
    {
        GameObject restPrefab = GetRestPrefab(duration);
        if (restPrefab == null || parent == null)
        {
            Debug.LogWarning($"❗ 지원되지 않는 쉼표 길이: {duration}분음표");
            return null;
        }

        GameObject rest = Instantiate(restPrefab, parent);
        RectTransform rt = rest.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        float spacing = MusicLayoutConfig.GetSpacing(parent);

        // 쉼표 위치 조정
        Vector2 offset = GetRestVisualOffset(duration, spacing);
        rt.anchoredPosition = position + offset;

        // 쉼표 크기 조정
        Vector2 restSize = GetRestSizeByDuration(duration, spacing);
        rt.sizeDelta = restSize;
        rt.localScale = Vector3.one;

        Debug.Log($"🎵 쉼표 생성: {duration}분쉼표 at {rt.anchoredPosition}");
        return rest;
    }

    /// <summary>
    /// 음길이에 따른 쉼표 프리팹 선택
    /// </summary>
    public GameObject GetRestPrefab(int duration)
    {
        return duration switch
        {
            1 => rest1Prefab,
            2 => rest2Prefab,
            4 => rest4Prefab,
            8 => rest8Prefab,
            16 => rest16Prefab,
            _ => null
        };
    }

    /// <summary>
    /// 쉼표별 위치 조정
    /// </summary>
    private Vector2 GetRestVisualOffset(int duration, float spacing)
    {
        return duration switch
        {
            1 => new Vector2(0f, spacing * 0.7f),
            2 => new Vector2(0f, spacing * 0.3f),
            4 => new Vector2(0f, spacing * 0.3f),
            8 => new Vector2(0f, spacing * 0.0f),
            16 => new Vector2(0f, spacing * -0.4f),
            _ => new Vector2(0f, spacing * 1.5f)
        };
    }

    /// <summary>
    /// 쉼표별 크기 조정
    /// </summary>
    private Vector2 GetRestSizeByDuration(int duration, float spacing)
    {
        return duration switch
        {
            1 => new Vector2(spacing * 1.5f, spacing * 0.5f),
            2 => new Vector2(spacing * 1.5f, spacing * 0.5f),
            4 => new Vector2(spacing * 1.0f, spacing * 3.0f),
            8 => new Vector2(spacing * 1.0f, spacing * 1.6f),
            16 => new Vector2(spacing * 1.0f, spacing * 2.3f),
            _ => new Vector2(spacing, spacing)
        };
    }

    /// <summary>
    /// 프리팹 유효성 검사
    /// </summary>
    public bool ValidatePrefabs()
    {
        bool isValid = true;
        if (rest1Prefab == null) { Debug.LogWarning("⚠️ rest1Prefab이 없습니다"); isValid = false; }
        if (rest2Prefab == null) { Debug.LogWarning("⚠️ rest2Prefab이 없습니다"); isValid = false; }
        if (rest4Prefab == null) { Debug.LogWarning("⚠️ rest4Prefab이 없습니다"); isValid = false; }
        if (rest8Prefab == null) { Debug.LogWarning("⚠️ rest8Prefab이 없습니다"); isValid = false; }
        if (rest16Prefab == null) { Debug.LogWarning("⚠️ rest16Prefab이 없습니다"); isValid = false; }
        return isValid;
    }
}
