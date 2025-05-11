using UnityEngine;

/// <summary>
/// 악보 데이터를 기반으로 음표를 생성하는 컴포넌트
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Song
    {
        public string title;
        public string clef;         // 음자리표
        public string time;         // 박자표
        public string[] notes;
    }

    [Header("🎼 Clefs")]
    [SerializeField] private GameObject clefTreblePrefab;
    [SerializeField] private GameObject clefBassPrefab;

    [Header("🕓 Time Signatures")]
    [SerializeField] private GameObject timeSig_2_4_Prefab;
    [SerializeField] private GameObject timeSig_3_4_Prefab;
    [SerializeField] private GameObject timeSig_4_4_Prefab;
    [SerializeField] private GameObject timeSig_3_8_Prefab;
    [SerializeField] private GameObject timeSig_4_8_Prefab;
    [SerializeField] private GameObject timeSig_6_8_Prefab;

    [Header("🛠 Time Signature Settings")]
    [SerializeField] private Vector2 timeSignaturePosition = new Vector2(100f, 0f);
    [SerializeField] private float timeSignatureWidth = 48f;

    [Header("🎼 Treble Clef Settings")]
    [SerializeField] private Vector2 trebleClefPosition = new Vector2(30f, -115f);
    [SerializeField] private Vector2 trebleClefSize = new Vector2(140f, 280f);

    [Header("🎼 Bass Clef Settings")]
    [SerializeField] private Vector2 bassClefPosition = new Vector2(30f, -115f);
    [SerializeField] private Vector2 bassClefSize = new Vector2(140f, 280f);

    [Header("Helpers")]
    [SerializeField] private NotePrefabProvider prefabProvider;

    [Header("표시 UI")]
    [SerializeField] private RectTransform linesContainer;
    [SerializeField] private RectTransform notesContainer;

    [Header("파일 데이터")]
    [SerializeField] private TextAsset songsJson;
    [SerializeField] private int selectedSongIndex = 0;

    [Header("설정")]
    [SerializeField] private float staffHeight = 150f;
    [SerializeField] private float beatSpacing = 80f;
    [SerializeField] private float noteYOffset = 0f;
    [SerializeField] private float ledgerYOffset = 0f;
    [SerializeField] private float noteScale = 2f;
    [SerializeField] private float wholeNoteYOffset = 0f;

    private NoteDataLoader dataLoader;
    private NoteMapper noteMapper;
    private LedgerLineHelper ledgerHelper;

    private void Awake()
    {
        dataLoader = new NoteDataLoader(songsJson);
        noteMapper = new NoteMapper();
        ledgerHelper = new LedgerLineHelper(prefabProvider.ledgerLinePrefab, notesContainer);
    }

    private void Start()
    {
        var songList = dataLoader.LoadSongs();

        if (songList == null || songList.songs == null || songList.songs.Length == 0)
        {
            Debug.LogError("[NoteSpawner] songs 배열이 비어 있거나 JSON 파싱 실패");
            return;
        }

        if (selectedSongIndex < 0 || selectedSongIndex >= songList.songs.Length)
        {
            Debug.LogError($"[NoteSpawner] selectedSongIndex ({selectedSongIndex}) 가 songs 배열 범위를 벗어남");
            return;
        }

        var song = songList.songs[selectedSongIndex];

        SpawnClef(song.clef);
        SpawnTimeSignature(song.time);
        ClearNotes();
        SpawnSongNotes();
    }

    private void SpawnClef(string clefType)
    {
        GameObject clefPrefab = clefType == "Bass" ? clefBassPrefab : clefTreblePrefab;

        if (clefPrefab == null)
        {
            Debug.LogWarning("[NoteSpawner] Clef prefab이 설정되지 않았습니다.");
            return;
        }

        var clef = Instantiate(clefPrefab, linesContainer);
        var rt = clef.GetComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);

        if (clefType == "Bass")
        {
            rt.anchoredPosition = bassClefPosition;
            rt.sizeDelta = bassClefSize;
        }
        else
        {
            rt.anchoredPosition = trebleClefPosition;
            rt.sizeDelta = trebleClefSize;
        }
    }

    private void SpawnTimeSignature(string time)
    {
        GameObject prefab = time switch
        {
            "2/4" => timeSig_2_4_Prefab,
            "3/4" => timeSig_3_4_Prefab,
            "4/4" => timeSig_4_4_Prefab,
            "3/8" => timeSig_3_8_Prefab,
            "4/8" => timeSig_4_8_Prefab,
            "6/8" => timeSig_6_8_Prefab,
            _ => null
        };

        if (prefab == null)
        {
            Debug.LogWarning($"[NoteSpawner] ❗ 등록되지 않은 박자표: {time}");
            return;
        }

        var obj = Instantiate(prefab, linesContainer);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);

        rt.anchoredPosition = timeSignaturePosition;
        rt.sizeDelta = new Vector2(timeSignatureWidth, staffHeight); // 오선 높이만큼 세로 채우기
    }

    private void ClearNotes()
    {
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
            Destroy(notesContainer.GetChild(i).gameObject);
    }

    private void SpawnSongNotes()
    {
        // 생략
    }

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