using UnityEngine;

/// <summary>
/// 음표 플래그(flag) 생성 전용 컴포넌트
/// </summary>
public class NoteFlagCreator : MonoBehaviour
{
    [Header("플래그 프리팹")]
    public GameObject flag8Prefab;
    public GameObject flag16Prefab;

    /// <summary>
    /// 플래그 붙이기 (스템과 음높이 기준)
    /// </summary>
    public GameObject AttachFlag(GameObject stem, int duration, float noteIndex, RectTransform parent)
    {
        GameObject flagPrefab = GetFlagPrefab(duration);
        if (flagPrefab == null || stem == null)
        {
            if (duration >= 8) // 8분음표 이상만 경고
                Debug.LogWarning($"❗ {duration}분음표에 대한 플래그 프리팹이 없습니다.");
            return null;
        }

        float spacing = MusicLayoutConfig.GetSpacing(parent);
        
        GameObject flag = Instantiate(flagPrefab, stem.transform);
        RectTransform flagRT = flag.GetComponent<RectTransform>();

        // B4(0) 이상의 음표는 꼬리가 아래로 향함
        bool stemDown = noteIndex >= 0f;

        if (stemDown)
        {
            // 꼬리가 아래로: 플래그를 stem의 아래쪽 끝에 배치
            flagRT.anchorMin = flagRT.anchorMax = new Vector2(1f, 0f);
            flagRT.pivot = new Vector2(0f, 1f);
            float flagXOffset = spacing * 0.0f;
            float flagYOffset = spacing * 0.1f;
            flagRT.anchoredPosition = new Vector2(flagXOffset, flagYOffset);
            flagRT.localScale = new Vector3(1f, -1f, 1f); // Y축 뒤집기
        }
        else
        {
            // 꼬리가 위로: 플래그를 stem의 위쪽 끝에 배치
            flagRT.anchorMin = flagRT.anchorMax = new Vector2(0f, 1f);
            flagRT.pivot = new Vector2(0f, 1f);
            float flagXOffset = spacing * 0.05f;
            float flagYOffset = spacing * -0.1f;
            flagRT.anchoredPosition = new Vector2(flagXOffset, flagYOffset);
            flagRT.localScale = Vector3.one;
        }

        flagRT.sizeDelta = new Vector2(
            spacing * MusicLayoutConfig.FlagSizeXRatio, 
            spacing * MusicLayoutConfig.FlagSizeYRatio
        );
        
        Debug.Log($"🎏 Flag 생성: duration={duration}, stemDown={stemDown}");
        return flag;
    }

    /// <summary>
    /// 음길이에 따른 플래그 프리팹 선택
    /// </summary>
    public GameObject GetFlagPrefab(int duration)
    {
        return duration switch
        {
            8 => flag8Prefab,
            16 => flag16Prefab,
            _ => null
        };
    }

    /// <summary>
    /// 프리팹 유효성 검사
    /// </summary>
    public bool ValidatePrefabs()
    {
        bool isValid = true;
        if (flag8Prefab == null) { Debug.LogWarning("⚠️ flag8Prefab이 없습니다"); isValid = false; }
        if (flag16Prefab == null) { Debug.LogWarning("⚠️ flag16Prefab이 없습니다"); isValid = false; }
        return isValid;
    }
}
