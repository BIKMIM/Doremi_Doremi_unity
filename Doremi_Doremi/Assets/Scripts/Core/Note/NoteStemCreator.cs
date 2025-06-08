using UnityEngine;

/// <summary>
/// 음표 스템(stem) 생성 전용 컴포넌트
/// </summary>
public class NoteStemCreator : MonoBehaviour
{
    [Header("Stem 프리팹")]
    public GameObject stemPrefab;

    /// <summary>
    /// 스템 붙이기 (음표 머리와 음높이 기준)
    /// </summary>
    public GameObject AttachStem(GameObject head, float noteIndex, RectTransform parent)
    {
        if (head == null || stemPrefab == null)
        {
            Debug.LogError("❌ Stem 생성 실패: head 또는 stemPrefab이 null");
            return null;
        }

        float spacing = MusicLayoutConfig.GetSpacing(parent);
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;
        float stemWidth = spacing * 0.2f;
        float stemHeight = spacing * 3f;

        GameObject stem = Instantiate(stemPrefab, head.transform);
        RectTransform stemRT = stem.GetComponent<RectTransform>();

        stemRT.anchorMin = stemRT.anchorMax = new Vector2(0.5f, 0.5f);

        // B4(0) 이상의 음표는 꼬리가 아래로 향함
        bool stemDown = noteIndex >= 0f;

        if (stemDown)
        {
            // 꼬리가 아래로: 머리 왼쪽에서 아래로
            stemRT.pivot = new Vector2(1f, 1f); // 우상단 기준
            float xOffset = headWidth * 0.35f;
            float yOffset = spacing * 0.1f;
            stemRT.anchoredPosition = new Vector2(-xOffset, -yOffset);
        }
        else
        {
            // 꼬리가 위로: 머리 오른쪽에서 위로
            stemRT.pivot = new Vector2(0f, 0f); // 좌하단 기준
            float xOffset = headWidth * 0.35f;
            float yOffset = spacing * 0.1f;
            stemRT.anchoredPosition = new Vector2(xOffset, yOffset);
        }

        stemRT.sizeDelta = new Vector2(stemWidth, stemHeight);
        stemRT.localScale = Vector3.one;

        Debug.Log($"🦴 Stem 생성: noteIndex={noteIndex}, stemDown={stemDown}");
        return stem;
    }

    /// <summary>
    /// 프리팹 유효성 검사
    /// </summary>
    public bool ValidatePrefab()
    {
        if (stemPrefab == null)
        {
            Debug.LogWarning("⚠️ stemPrefab이 없습니다");
            return false;
        }
        return true;
    }
}
