using UnityEngine;
using UnityEngine.UI;

public class NoteAssembler : MonoBehaviour
{
    [Header("오선 패널 (Canvas 내부)")]
    public RectTransform staffPanel;

    [Header("음표 머리 프리팹")]
    public GameObject head1Prefab; // 1분음표
    public GameObject head2Prefab; // 2분음표
    public GameObject head4Prefab; // 4분음표

    [Header("Stem 프리팹")]
    public GameObject stemPrefab;

    [Header("🎏 Flag 프리팹")]
    public GameObject flagPrefab;


    // 🎵 1. 머리 생성 함수
    public GameObject SpawnNoteHead(GameObject prefab, Vector2 anchoredPos)
    {
        GameObject head = Instantiate(prefab, staffPanel); 
        RectTransform rt = head.GetComponent<RectTransform>(); 

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float noteHeadWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;
        float noteHeadHeight = spacing * MusicLayoutConfig.NoteHeadHeightRatio;
        rt.sizeDelta = new Vector2(noteHeadWidth, noteHeadHeight);
        rt.localScale = Vector3.one;

        return head;
    }

    // 🦴 2. 스템 붙이기 함수 (머리를 받아서 붙임)
    public GameObject AttachStem(GameObject head)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;
        float stemWidth = spacing * 0.2f;
        float stemHeight = spacing * 3f;

        GameObject stem = Instantiate(stemPrefab, head.transform);
        RectTransform stemRT = stem.GetComponent<RectTransform>();

        stemRT.anchorMin = new Vector2(0.5f, 0.5f);
        stemRT.anchorMax = new Vector2(0.5f, 0.5f);
        stemRT.pivot = new Vector2(0f, 0f); // 좌측 중앙 기준

        stemRT.anchoredPosition = new Vector2(headWidth / 3f, 0f);
        stemRT.sizeDelta = new Vector2(stemWidth, stemHeight);
        stemRT.localScale = Vector3.one;

        return stem;
    }


    // 🎏 3. 플래그 붙이기 함수 (스템을 받아서 붙임)
    public GameObject AttachFlag(GameObject stem)
    {
        RectTransform stemRT = stem.GetComponent<RectTransform>();
        float stemHeight = stemRT.sizeDelta.y; // ✅ 진짜 높이 읽기
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel); // 필요하면 크기 비례용

        GameObject flag = Instantiate(flagPrefab, stem.transform);
        RectTransform flagRT = flag.GetComponent<RectTransform>();

        flagRT.anchorMin = new Vector2(0f, 1f);
        flagRT.anchorMax = new Vector2(0f, 1f);
        flagRT.pivot = new Vector2(0f, 1f); // 좌측 상단 기준.3
        
        flagRT.anchoredPosition = new Vector2(0f, spacing * MusicLayoutConfig.FlagOffsetRatio * -0.1f); // stem 위에 딱 붙게
        flagRT.sizeDelta = new Vector2(spacing * MusicLayoutConfig.FlagSizeXRatio, spacing * MusicLayoutConfig.FlagSizeYRatio); // 꼬리 길이
        flagRT.localScale = Vector3.one;

        return flag;
    }


    // ✅ 최종 조립 함수: 머리 → 스템 → 플래그
    public void SpawnNoteFull(Vector2 anchoredPos)
{
    GameObject head = SpawnNoteHead(head4Prefab, anchoredPos); 
    GameObject stem = AttachStem(head);
    GameObject flag = AttachFlag(stem);
}

}
