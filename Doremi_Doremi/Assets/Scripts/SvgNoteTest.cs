using UnityEngine;
using UnityEngine.UI;

public class SvgNoteTest : MonoBehaviour
{
    [Header("오선 패널 (Canvas 내부)")]
    public RectTransform staffPanel;

    [Header("음표 머리 프리팹")]
    public GameObject head1Prefab; // 1분음표
    public GameObject head2Prefab; // 2분음표
    public GameObject head4Prefab; // 4분음표

    [Header("Stem 프리팹")]
    public GameObject stemPrefab;

    void Start()
    {
        SpawnNoteHead(head2Prefab, new Vector2(0f, 0f));
        SpawnNoteWithStem();
    }

    GameObject SpawnNoteHead(GameObject prefab, Vector2 anchoredPos)
    {
        GameObject head = Instantiate(prefab, staffPanel); 
        RectTransform rt = head.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel); // 각 줄 사이의 간격 계산.
        float noteHeadWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio; // 음표 머리 너비 계산.
        float noteHeadHeight = spacing * MusicLayoutConfig.NoteHeadHeightRatio; // 음표 머리 높이 계산.

        rt.sizeDelta = new Vector2(noteHeadWidth, noteHeadHeight); // 음표 머리 크기 설정.

        rt.localScale = Vector3.one;

        return head; //생성된 머리head를 반환. 따로 조립 가능하게끔.
    }

    void SpawnNoteWithStem()
    {
        GameObject head = SpawnNoteHead(head4Prefab, new Vector2(0f, 0f));
        RectTransform headRT = head.GetComponent<RectTransform>();

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;

        GameObject stem = Instantiate(stemPrefab, head.transform);
        RectTransform stemRT = stem.GetComponent<RectTransform>(); 

        stemRT.anchorMin = new Vector2(0.5f, 0.5f); // min, max 값이 둘다 0.5면 완전 정중앙이 이동중심점이 됨.
        stemRT.anchorMax = new Vector2(0.5f, 0.5f); 
        stemRT.pivot = new Vector2(0f, 0f); // 스템의 피벗을 왼쪽 아래로 설정. 음표 머리와 연결될 부분이 피벗이 되도록 설정.이건 주로 회전축.
        stemRT.anchoredPosition = new Vector2(headWidth / 3f, 0f); // 음표 머리를 3으로 나눈만큼 오른쪽에 stem이 위치하도록 설정. y값은 0.

        float stemWidth = spacing * 0.2f; // 예: 줄 간격의 20%
        float stemHeight = spacing * 3f; // 이건 stem이 오선지 3칸만한 높이까지 올라간다는 뜻.

        stemRT.sizeDelta = new Vector2(stemWidth, stemHeight);

        stemRT.localScale = Vector3.one;
    }


}
