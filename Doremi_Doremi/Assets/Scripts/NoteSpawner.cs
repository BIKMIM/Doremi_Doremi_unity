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
        public string clef;         // 혼방의 음자리표 (토리, 다른 보석)
        public string[] notes;
    }

    [Header("🎼 Treble Clef Settings")]
    [SerializeField] private Vector2 trebleClefPosition = new Vector2(30f, -115f);
    [SerializeField] private Vector2 trebleClefSize = new Vector2(140f, 280f);

    [Header("🎼 Bass Clef Settings")]
    [SerializeField] private Vector2 bassClefPosition = new Vector2(30f, -115f);
    [SerializeField] private Vector2 bassClefSize = new Vector2(140f, 280f);



    [Header("혼방의 자리표 프리파브")]
    [SerializeField] private GameObject clefTreblePrefab;
    [SerializeField] private GameObject clefBassPrefab;

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

        // 🛡 songs 배열 비어 있는지 확인
        if (songList == null || songList.songs == null || songList.songs.Length == 0)
        {
            Debug.LogError("[NoteSpawner] songs 배열이 비어 있거나 JSON 파싱 실패");
            return;
        }

        // 🛡 selectedSongIndex가 범위 초과인지 확인
        if (selectedSongIndex < 0 || selectedSongIndex >= songList.songs.Length)
        {
            Debug.LogError($"[NoteSpawner] selectedSongIndex ({selectedSongIndex}) 가 songs 배열 범위를 벗어남");
            return;
        }

        var song = songList.songs[selectedSongIndex];

        SpawnClef(song.clef);
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

        // 🎯 Clef 타입에 따라 위치/크기 적용
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



    private void ClearNotes()
    {
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
            Destroy(notesContainer.GetChild(i).gameObject);
    }

    private void SpawnSongNotes()
    {
        var songList = dataLoader.LoadSongs();
        var song = songList.songs[selectedSongIndex];
        var notes = song.notes;

        float totalSpan = beatSpacing * (notes.Length - 1);
        float currentX = -totalSpan / 2f;

        var midLineRT = linesContainer.GetChild(2).GetComponent<RectTransform>();
        float baselineY = midLineRT.anchoredPosition.y;
        float spacing = staffHeight / 4f;

        foreach (var token in notes)
        {
            var parts = token.Split(':');
            string pitch = parts[0];
            string code = parts.Length > 1 ? parts[1] : "4";
            bool isRest = pitch == "R";

            float index = 0f;
            bool stemDown = false;

            if (!isRest && noteMapper.TryGetIndex(pitch, out index))
            {
                stemDown = code == "1" ? false : index >= 1.5f;
            }

            float y = baselineY + index * spacing + noteYOffset;

            if (!isRest)
            {
                ledgerHelper.GenerateLedgerLines(index, baselineY, spacing, currentX, ledgerYOffset);
            }

            if (code == "1")
            {
                y += wholeNoteYOffset;
                y += 20f;
                y -= spacing * 2f;
            }

            if (isRest)
            {
                var rest = Instantiate(prefabProvider.GetRest(code), notesContainer);
                var restRt = rest.GetComponent<RectTransform>();
                restRt.anchorMin = restRt.anchorMax = new Vector2(0.5f, 0.5f);
                restRt.pivot = new Vector2(0.5f, 0.5f);
                restRt.localScale = Vector3.one * noteScale;
                restRt.anchoredPosition = new Vector2(currentX, baselineY);
            }
            else
            {
                var head = prefabProvider.GetNoteHead(code);
                var stem = (code == "1") ? null : prefabProvider.noteStemPrefab;
                GameObject flag = null;
                if (code == "8") flag = prefabProvider.noteFlag8Prefab;
                if (code == "16") flag = prefabProvider.noteFlag16Prefab;

                var wrap = NoteFactory.CreateNoteWrap(
                    notesContainer,
                    head,
                    stem,
                    flag,
                    null,
                    stemDown,
                    new Vector2(currentX, y),
                    noteScale
                );
            }

            currentX += beatSpacing * GetBeatLength(code);
        }
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