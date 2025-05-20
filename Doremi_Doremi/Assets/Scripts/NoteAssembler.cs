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

    [Header("🎯 Dot 프리팹")]
    public GameObject dotPrefab;


    // 🎵 1. 머리 생성 함수
    public GameObject SpawnNoteHead(GameObject prefab, Vector2 anchoredPos)
    {
        GameObject head = Instantiate(prefab, staffPanel); // staffPanel에 붙여서 생성
        RectTransform rt = head.GetComponent<RectTransform>();  

        rt.anchorMin = new Vector2(0.5f, 0.5f); 
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel); // 줄 간격 계산
        float noteHeadWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio; 
        float noteHeadHeight = spacing * MusicLayoutConfig.NoteHeadHeightRatio;
        rt.sizeDelta = new Vector2(noteHeadWidth, noteHeadHeight);
        rt.localScale = Vector3.one;

        return head;
    }

    // 🦴 2. 스템 붙이기 함수 (머리를 받아서 붙임)
    public GameObject AttachStem(GameObject head)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel); // 줄 간격 계산
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;
        float stemWidth = spacing * 0.2f; // 스템 너비 비율
        float stemHeight = spacing * 3f; // 스템 높이 비율

        GameObject stem = Instantiate(stemPrefab, head.transform); 
        RectTransform stemRT = stem.GetComponent<RectTransform>();

        stemRT.anchorMin = new Vector2(0.5f, 0.5f); 
        stemRT.anchorMax = new Vector2(0.5f, 0.5f);
        stemRT.pivot = new Vector2(0f, 0f); // 좌측 중앙 기준

        stemRT.anchoredPosition = new Vector2(headWidth / 3f, 0f); // 머리 오른쪽에 붙게
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

    // 4. 점 붙이기 함수 (머리를 받아서 붙임)
    public GameObject AttachDot(GameObject head, bool isOnLine)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float dotSize = spacing * 0.3f; // 점 크기 (줄 간격의 1/4)
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;

        GameObject dot = Instantiate(dotPrefab, head.transform);
        RectTransform dotRT = dot.GetComponent<RectTransform>();

        dotRT.anchorMin = new Vector2(0.5f, 0.5f);
        dotRT.anchorMax = new Vector2(0.5f, 0.5f);
        dotRT.pivot = new Vector2(0f, 0.5f); // 왼쪽 가운데 기준


        float x = headWidth + spacing * 0.1f;
        float y = isOnLine ? spacing * 0.3f : spacing * -0.0f; // 라인에 걸쳐있으면 앞쪽, 아니면 뒤쪽 spacing 수정.


        dotRT.anchoredPosition = new Vector2(x, y);
        dotRT.sizeDelta = new Vector2(dotSize, dotSize);
        dotRT.localScale = Vector3.one;

        return dot;
    }


    // ✅ 최종 조립 함수: 머리 → 스템 → 플래그
    public void SpawnNoteFull(Vector2 anchoredPos)
{
    GameObject head = SpawnNoteHead(head4Prefab, anchoredPos);   
    GameObject stem = AttachStem(head);
    GameObject flag = AttachFlag(stem);

    }

    public void SpawnDottedNoteFull(Vector2 anchoredPos, float noteIndex, bool isOnLine)
    {
        GameObject head = SpawnNoteHead(head4Prefab, anchoredPos);
        GameObject stem = AttachStem(head);
        GameObject flag = AttachFlag(stem);
        GameObject dot = AttachDot(head, isOnLine); // 점은 머리 위에 붙임
    
    }

}
