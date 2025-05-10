using UnityEngine;

/// <summary>
/// 🎵 JSON 기반 악보를 읽고 음표 프리팹을 생성하는 메인 컴포넌트
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [Header("Helpers")]
    [SerializeField] private NotePrefabProvider prefabProvider;

    [Header("🎹 UI")]
    public RectTransform staffPanel;             // 오선 패널
    public RectTransform notesContainer;         // 음표가 배치될 부모 오브젝트

    [Header("📄 Data")]
    public TextAsset songsJson;                  // JSON 악보 데이터
    public int selectedSongIndex = 0;

    [Header("⚙ Settings")]
    public float staffHeight = 150f;             // 오선 전체 높이
    public float noteYOffset = 0f;               // 기본 Y 위치 보정값
    public float ledgerYOffset = 4f;             // 덧줄 위치 미세 보정
    public float beatSpacing = 80f;              // 음표 간 X 간격
    public float stemDownYOffset = 6f;           // stemDown 프리팹 Y 위치 추가 보정

    // 내부 도우미
    private NoteDataLoader dataLoader;
    private NoteMapper noteMapper;
    private LedgerLineHelper ledgerHelper;

    private void Awake()
    {
        if (songsJson == null)
        {
            Debug.LogError("[NoteSpawner] 🎵 songsJson이 할당되지 않았습니다.");
            return;
        }

        if (prefabProvider == null)
        {
            Debug.LogError("[NoteSpawner] 🎯 NotePrefabProvider가 연결되지 않았습니다.");
            return;
        }

        dataLoader = new NoteDataLoader(songsJson);
        noteMapper = new NoteMapper();
        ledgerHelper = new LedgerLineHelper(prefabProvider.ledgerLinePrefab, notesContainer);
    }

    private void Start()
    {
        ClearNotes();
        SpawnSongNotes();
    }

    /// <summary> 기존 음표 제거 </summary>
    private void ClearNotes()
    {
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(notesContainer.GetChild(i).gameObject);
        }
    }

    /// <summary> 악보에서 음표를 생성 </summary>
    private void SpawnSongNotes()
    {
        var songList = dataLoader.LoadSongs();
        var song = songList.songs[selectedSongIndex];

        float spacing = staffHeight / 4f;      // 오선 두 줄 간격 (줄-줄)
        float baseY = -108f;                   // G4 (index = 0f)가 두 번째 줄에 위치하도록 설정
        float currentX = -200f;

        foreach (var token in song.notes)
        {
            string[] parts = token.Split(':');
            string pitch = parts[0];
            string code = parts.Length > 1 ? parts[1].Trim() : "4";
            bool isRest = pitch == "R";

            float index = 0f;
            bool stemDown = false;

            if (!isRest && noteMapper.TryGetIndex(pitch, out index))
            {
                stemDown = index >= 2.5f; // B4 이상부터 stemDown 처리
            }

            GameObject prefab = prefabProvider.GetPrefab(code, stemDown);
            if (prefab == null)
            {
                Debug.LogWarning($"[NoteSpawner] ❌ 알 수 없는 음표 코드: {code}");
                continue;
            }

            GameObject note = Instantiate(prefab, notesContainer);
            RectTransform rt = note.GetComponent<RectTransform>();

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0.5f);  // 프리팹 기준을 정중앙으로 설정

            float y = isRest
                ? baseY + noteYOffset
                : Mathf.Round(baseY + index * spacing + noteYOffset + (stemDown ? stemDownYOffset : 0f));

            rt.anchoredPosition = new Vector2(currentX, y);
            rt.localRotation = Quaternion.identity; // 회전 제거: 프리팹이 이미 올바른 방향이라면 회전 불필요

            if (!isRest)
            {
                ledgerHelper.GenerateLedgerLines(index, baseY, spacing, currentX, ledgerYOffset);
            }

            currentX += beatSpacing * GetBeatLength(code);
        }
    }

    /// <summary> 음표 코드에 따른 길이 계산 </summary>
    private float GetBeatLength(string code)
    {
        return code switch
        {
            "1" => 2f,
            "2" => 2f,
            "4" => 1.5f,
            "8" => 1f,
            "16" => 1f,
            "1R" => 2f,
            "2R" => 2f,
            "4R" => 1.5f,
            "8R" => 1f,
            "16R" => 1f,
            _ => 1f
        };
    }
}
