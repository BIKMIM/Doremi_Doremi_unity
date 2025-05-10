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
    public RectTransform notesContainer;         // 음표들이 배치될 부모

    [Header("📄 Data")]
    public TextAsset songsJson;                  // JSON 악보
    public int selectedSongIndex = 0;            // 몇 번째 곡을 선택할지

    [Header("⚙ Settings")]
    public float staffHeight = 150f;             // 오선 높이
    public float noteYOffset = -10f;             // 음표 위치 보정
    public float ledgerYOffset = 4f;             // 덧줄 위치 보정
    public float beatSpacing = 80f;              // 박자 간격

    // 도우미 클래스
    private NoteDataLoader dataLoader;
    private NoteMapper noteMapper;
    private LedgerLineHelper ledgerHelper;

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
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

    /// <summary>
    /// 실행 시 음표 생성
    /// </summary>
    private void Start()
    {
        ClearNotes();
        SpawnSongNotes();
    }

    /// <summary>
    /// 기존 음표 전부 제거
    /// </summary>
    private void ClearNotes()
    {
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(notesContainer.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// JSON에서 선택된 곡의 음표를 생성
    /// </summary>
    private void SpawnSongNotes()
    {
        var songList = dataLoader.LoadSongs();
        var song = songList.songs[selectedSongIndex];

        float spacing = staffHeight / 4f;
        float baseY = Mathf.Round(staffPanel.anchoredPosition.y);
        float currentX = -200f;

        foreach (var token in song.notes)
        {
            string[] parts = token.Split(':');
            string pitch = parts[0];
            string code = parts.Length > 1 ? parts[1].Trim() : "4";
            bool isRest = pitch == "R";

            float index = 0f;
            bool stemDown = !isRest && noteMapper.TryGetIndex(pitch, out index) && index > 2f;

            GameObject prefab = prefabProvider.GetPrefab(code, stemDown);
            if (prefab == null)
            {
                Debug.LogWarning($"[NoteSpawner] ⛔ 알 수 없는 음표 코드: {code}");
                continue;
            }

            GameObject note = Instantiate(prefab, notesContainer);
            RectTransform rt = note.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);

            float y = isRest
                ? baseY + noteYOffset
                : Mathf.Round(baseY + index * spacing + noteYOffset);

            rt.anchoredPosition = new Vector2(currentX, y);

            if (!isRest)
            {
                ledgerHelper.GenerateLedgerLines(index, baseY, spacing, currentX, ledgerYOffset);
            }

            currentX += beatSpacing * GetBeatLength(code);
        }
    }

    /// <summary>
    /// 코드에 따른 음표 길이 반환
    /// </summary>
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
