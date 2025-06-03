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

    // 쉼표 별로 위치 조정
    private Vector2 GetRestVisualOffset(int duration, float spacing)
    {
        return duration switch
        {
            1 => new Vector2(spacing * 0f, spacing * 0.7f),  // 1분 쉼표는 아래로 살짝
            2 => new Vector2(spacing * 0f, spacing * 0.3f),   // 2분 쉼표는 위로 살짝
            4 => new Vector2(spacing * 0f, spacing * 0.3f),   // 4분 쉼표는 위로 살짝
            8 => new Vector2(spacing * 0f, spacing * 0.0f),   // 8분 쉼표는 위로 살짝
            16 => new Vector2(spacing * 0f, spacing * -0.4f),   // 16분 쉼표는 위로 살짝
            _ => new Vector2(0f, spacing * 1.5f)    // 그 외는 오선 중앙보다 위
        };
    }

    // 쉼표별 크기 조정
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

    // 쉼표 생성 함수
    public void SpawnRestNote(Vector2 basePos, int duration, bool isDotted)
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

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);

        // ✅ 쉼표 위치 조정
        Vector2 offset = GetRestVisualOffset(duration, spacing);
        rt.anchoredPosition = basePos + offset;

        // ✅ 쉼표 크기 조정
        Vector2 restSize = GetRestSizeByDuration(duration, spacing);
        rt.sizeDelta = restSize;

        rt.localScale = Vector3.one;

        if (isDotted)
        {
            AttachDot(rest, isOnLine: false);
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

    // 🦴 2. 스템 붙이기 함수 (머리와 음높이를 받아서 붙임)
    public GameObject AttachStem(GameObject head, float noteIndex)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel); // 줄 간격 계산
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;
        float stemWidth = spacing * 0.2f; // 스템 너비 비율
        float stemHeight = spacing * 3f; // 스템 높이 비율

        GameObject stem = Instantiate(stemPrefab, head.transform);
        RectTransform stemRT = stem.GetComponent<RectTransform>();

        stemRT.anchorMin = new Vector2(0.5f, 0.5f);
        stemRT.anchorMax = new Vector2(0.5f, 0.5f);

        // ✅ B4(0) 이상의 음표는 꼬리가 아래로 향함
        bool stemDown = noteIndex >= 0f; // B4 이상

        if (stemDown)
        {
            // 꼬리가 아래로: 머리 왼쪽에서 아래로
            stemRT.pivot = new Vector2(1f, 1f); // 우상단 기준
            // ✅ 머리 왼쪽에서 시작해서 적절한 거리만큼 아래로
            float xOffset = headWidth * 0.35f; // 머리 폭의 35% 왼쪽
            float yOffset = spacing * 0.1f;    // 머리에서 살짝 아래서 시작
            stemRT.anchoredPosition = new Vector2(-xOffset, -yOffset);
            stemRT.sizeDelta = new Vector2(stemWidth, stemHeight);
            stemRT.localScale = Vector3.one;
        }
        else
        {
            // 꼬리가 위로: 머리 오른쪽에서 위로 (기존 방식)
            stemRT.pivot = new Vector2(0f, 0f); // 좌하단 기준
            float xOffset = headWidth * 0.35f; // 머리 폭의 35% 오른쪽
            float yOffset = spacing * 0.1f;    // 머리에서 살짝 위에서 시작
            stemRT.anchoredPosition = new Vector2(xOffset, yOffset);
            stemRT.sizeDelta = new Vector2(stemWidth, stemHeight);
            stemRT.localScale = Vector3.one;
        }

        Debug.Log($"🦴 Stem 생성: noteIndex={noteIndex}, stemDown={stemDown}, position={stemRT.anchoredPosition}, size={stemRT.sizeDelta}");

        return stem;
    }

    // 🎏 3. 플래그 붙이기 함수 (스템과 음높이를 받아서 붙임)
    public void AttachFlag(GameObject stem, int duration, float noteIndex)
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

        // ✅ B4(0) 이상의 음표는 꼬리가 아래로 향함
        bool stemDown = noteIndex >= 0f; // B4 이상

        if (stemDown)
        {
            // 꼬리가 아래로: 플래그를 stem의 아래쪽 끝에 배치
            flagRT.anchorMin = flagRT.anchorMax = new Vector2(1f, 0f); // ✅ 우하단으로 변경
            flagRT.pivot = new Vector2(0f, 1f); // 좌상단 기준
            float flagXOffset = spacing * 0.0f;  // stem 오른쪽으로 살짝
            float flagYOffset = spacing * 0.1f;  // 이제 이 값이 제대로 작동할 것입니다
            flagRT.anchoredPosition = new Vector2(flagXOffset, flagYOffset);
            flagRT.localScale = new Vector3(1f, -1f, 1f);
        }
        else
        {
            // 꼬리가 위로: 플래그를 stem의 위쪽 끝에 배치 (기존 방식)
            flagRT.anchorMin = flagRT.anchorMax = new Vector2(0f, 1f); // 좌상단
            flagRT.pivot = new Vector2(0f, 1f); // 좌상단 기준
            float flagXOffset = spacing * 0.05f; // spacing의 5%만큼 오른쪽
            float flagYOffset = spacing * -0.1f; // spacing의 10%만큼 아래로 조정
            flagRT.anchoredPosition = new Vector2(flagXOffset, flagYOffset);
            flagRT.localScale = Vector3.one;
        }

        flagRT.sizeDelta = new Vector2(spacing * MusicLayoutConfig.FlagSizeXRatio, spacing * MusicLayoutConfig.FlagSizeYRatio);
        
        Debug.Log($"🎏 Flag 생성: noteIndex={noteIndex}, stemDown={stemDown}, position={flagRT.anchoredPosition}, scale={flagRT.localScale}");
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
            GameObject stem = AttachStem(head, noteIndex); // ✅ noteIndex 전달

            if (duration >= 8)
            {
                AttachFlag(stem, duration, noteIndex); // ✅ noteIndex 전달
            }
        }
    }

    // 🎵 점음표 조립
    public void SpawnDottedNoteFull(Vector2 anchoredPos, float noteIndex, bool isOnLine, int duration)
    {
        GameObject head = SpawnNoteHead(GetHeadPrefab(duration), anchoredPos);

        if (duration >= 2)
        {
            GameObject stem = AttachStem(head, noteIndex); // ✅ noteIndex 전달

            if (duration >= 8)
            {
                AttachFlag(stem, duration, noteIndex); // ✅ noteIndex 전달
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