using UnityEngine;

/// <summary>
/// 음표 머리(head) 생성 전용 컴포넌트
/// </summary>
public class NoteHeadCreator : MonoBehaviour
{
    [Header("음표 머리 프리팹")]
    public GameObject head1Prefab; // 1분음표
    public GameObject head2Prefab; // 2분음표
    public GameObject head4Prefab; // 4분음표

    /// <summary>
    /// 음표 머리 생성
    /// </summary>
    public GameObject CreateNoteHead(GameObject prefab, Vector2 position, RectTransform parent)
    {
        if (prefab == null || parent == null)
        {
            Debug.LogError("❌ NoteHead 생성 실패: prefab 또는 parent가 null");
            return null;
        }

        GameObject head = Instantiate(prefab, parent);
        RectTransform rt = head.GetComponent<RectTransform>();

        // 앵커 및 피벗 설정
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;

        // 크기 설정
        float spacing = MusicLayoutConfig.GetSpacing(parent);
        float noteHeadWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;
        float noteHeadHeight = spacing * MusicLayoutConfig.NoteHeadHeightRatio;
        rt.sizeDelta = new Vector2(noteHeadWidth, noteHeadHeight);
        rt.localScale = Vector3.one;

        return head;
    }

    /// <summary>
    /// 음길이에 따른 머리 프리팹 선택
    /// </summary>
    public GameObject GetHeadPrefab(int duration)
    {
        return duration switch
        {
            1 => head1Prefab,
            2 => head2Prefab,
            4 => head4Prefab,
            _ => head4Prefab // 기본값
        };
    }

    /// <summary>
    /// 프리팹 유효성 검사
    /// </summary>
    public bool ValidatePrefabs()
    {
        bool isValid = true;
        if (head1Prefab == null) { Debug.LogWarning("⚠️ head1Prefab이 없습니다"); isValid = false; }
        if (head2Prefab == null) { Debug.LogWarning("⚠️ head2Prefab이 없습니다"); isValid = false; }
        if (head4Prefab == null) { Debug.LogWarning("⚠️ head4Prefab이 없습니다"); isValid = false; }
        return isValid;
    }
}
