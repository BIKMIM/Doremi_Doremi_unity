using UnityEngine;
using UnityEngine.UI;


// NoteAssembler.cs - 음표 조립(head, stem, flag, dot) 파일

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

    [Header("플래그 프리팹")]
    public GameObject flag8Prefab;
    public GameObject flag16Prefab;

    [Header("🎯 Dot 프리팹")]
    public GameObject dotPrefab;

    [Header("쉼표 프리팹")]
    public GameObject rest1Prefab; // 1분 쉼표 프리팹 
    public GameObject rest2Prefab; // 2분 쉼표 프리팹 
    public GameObject rest4Prefab; // 4분 쉼표 프리팹 
    public GameObject rest8Prefab; // 8분 쉼표 프리팹 
    public GameObject rest16Prefab; // 16분 쉼표 프리팹


    // 쉼표 생성 함수
    public void SpawnRestNote(Vector2 anchoredPos, int duration, bool isDotted)
    {
        GameObject restPrefab = GetRestPrefab(duration);
        if (restPrefab == null)
        {
            Debug.LogWarning($"❗ 지원되지 않는 쉼표 길이: {duration}분음표");
            return;
        }

        GameObject rest = Instantiate(restPrefab, staffPanel);
        RectTransform rt = rest.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float size = spacing * 0.9f;
        rt.sizeDelta = new Vector2(spacing * 1.0f, spacing * 3.0f);
        rt.localScale = Vector3.one;

        if (isDotted)
        {
            AttachDot(rest, isOnLine: false); // 쉼표는 줄에 안 걸려있으므로 false
        }
    }


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
    public void AttachFlag(GameObject stem, int duration)
    {
        GameObject flagPrefab = duration switch
        {
            8 => flag8Prefab,
            16 => flag16Prefab,
            _ => null
        };

        if (flagPrefab == null)
        {
            Debug.LogWarning($"❗ {duration}분음표에 대한 플래그 프리팹이 없습니다.");
            return;
        }

        RectTransform stemRT = stem.GetComponent<RectTransform>();
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);

        GameObject flag = Instantiate(flagPrefab, stem.transform);
        RectTransform flagRT = flag.GetComponent<RectTransform>();

        flagRT.anchorMin = flagRT.anchorMax = new Vector2(0f, 1f);
        flagRT.pivot = new Vector2(0f, 1f);
        flagRT.anchoredPosition = new Vector2(0f, spacing * MusicLayoutConfig.FlagOffsetRatio * -0.1f);
        flagRT.sizeDelta = new Vector2(spacing * MusicLayoutConfig.FlagSizeXRatio, spacing * MusicLayoutConfig.FlagSizeYRatio);
        flagRT.localScale = Vector3.one;
    }



    // 4. 점 붙이기 함수 (머리를 받아서 붙임)
    public GameObject AttachDot(GameObject headOrRest, bool isOnLine)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float dotSize = spacing * 0.3f;
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;

        GameObject dot = Instantiate(dotPrefab, headOrRest.transform);
        RectTransform rt = dot.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);

        // 🎯 위치 계산
        float x = headWidth + spacing * -0.4f; // 점음표 위치 지정.
        float y;

        if (isOnLine)
        {
            // 음표가 줄에 걸쳐 있을 때는 도트 위치를 위로 살짝
            y = spacing * 0.3f;
        }
        else
        {
            // 음표가 칸에 있을 때 또는 쉼표일 때는 동일하게 살짝 아래
            y = spacing * -0.1f;
        }

        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(dotSize, dotSize);
        rt.localScale = Vector3.one;

        return dot;
    }


    // ✅ 최종 조립 함수: 머리 → 스템 → 플래그
    // 🎵 음표 조립: 일반 음표
    public void SpawnNoteFull(Vector2 anchoredPos, float noteIndex, int duration)
    {
        GameObject head = SpawnNoteHead(GetHeadPrefab(duration), anchoredPos);

        if (duration >= 2)
        {
            GameObject stem = AttachStem(head); // ✅ stem 선언이 필요함

            if (duration >= 8)
            {
                AttachFlag(stem, duration); // ✅ duration 인자 넘겨줘야 함
            }
        }
    }


    // 🎵 점음표 조립
    public void SpawnDottedNoteFull(Vector2 anchoredPos, float noteIndex, bool isOnLine, int duration)
    {
        GameObject head = SpawnNoteHead(GetHeadPrefab(duration), anchoredPos);

        if (duration >= 2)
        {
            GameObject stem = AttachStem(head); // ✅ stem 선언

            if (duration >= 8)
            {
                AttachFlag(stem, duration); // ✅ duration 전달
            }
        }

        AttachDot(head, isOnLine);
    }


    // 🎵 머리 프리팹 선택
    private GameObject GetHeadPrefab(int duration)
    {
        return duration switch
        {
            1 => head1Prefab,
            2 => head2Prefab,
            4 => head4Prefab,
            _ => head4Prefab
        };
    }



    private GameObject GetRestPrefab(int duration)
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


}
