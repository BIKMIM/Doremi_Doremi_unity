using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Helpers")]
    [SerializeField] private NotePrefabProvider prefabProvider;

    [Header("🎹 UI")]
    [SerializeField] private RectTransform linesContainer; // StaffLineRenderer → linesContainer
    [SerializeField] private RectTransform notesContainer; // 음표가 올라갈 컨테이너

    [Header("📄 Data")]
    [SerializeField] private TextAsset songsJson;
    [SerializeField] private int selectedSongIndex = 0;

    [Header("⚙ Settings")]
    [SerializeField] private float staffHeight = 150f;  // 오선 전체 높이
    [SerializeField] private float beatSpacing = 80f;  // 음표 간 X 간격
    [SerializeField] private float noteYOffset = 0f;  // 음표 세로 오프셋
    [SerializeField] private float ledgerYOffset = 0f;  // 보조선 세로 오프셋
    [SerializeField] private float noteScale = 2f;  // 음표 스케일

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
        ClearNotes();
        SpawnSongNotes();
    }

    private void ClearNotes()
    {
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
            Destroy(notesContainer.GetChild(i).gameObject);
    }

    private void SpawnSongNotes()
    {
        // 1) 악보 읽기
        var songList = dataLoader.LoadSongs();
        var song = songList.songs[selectedSongIndex];
        var notes = song.notes;
        int count = notes.Length;

        // 2) 가로(시간)축 중앙 정렬
        float totalSpan = beatSpacing * (count - 1);
        float currentX = -totalSpan / 2f;

        // 3) 세로 기준선 계산
        //    linesContainer 자식(0~4) 중 [2]가 세 번째 오선입니다.
        var midLineRT = linesContainer.GetChild(2).GetComponent<RectTransform>();
        float baselineY = midLineRT.anchoredPosition.y;
        float spacing = staffHeight / 4f;  // 오선 5줄 → 간격 4칸

        // 4) 음표 생성 루프
        foreach (var token in notes)
        {
            var parts = token.Split(':');
            string pitch = parts[0];
            string code = parts.Length > 1 ? parts[1] : "4";
            bool isRest = pitch == "R";

            // 높이 인덱스 & 꼬리 방향
            float index = 0f;
            bool stemDown = false;
            if (!isRest && noteMapper.TryGetIndex(pitch, out index))
                stemDown = index >= -1f;

            // 음표 Prefab
            var prefab = prefabProvider.GetPrefab(code, stemDown);
            if (prefab == null)
            {
                Debug.LogWarning($"Unknown code: {code}");
                currentX += beatSpacing * GetBeatLength(code);
                continue;
            }

            // 생성 & 세팅
            var note = Instantiate(prefab, notesContainer);
            var rt = note.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one * noteScale;
            rt.localRotation = stemDown
                               ? Quaternion.Euler(0, 0, 180f)
                               : Quaternion.identity;

            // Y 위치: baselineY + index*spacing + noteYOffset
            float y = baselineY + index * spacing + noteYOffset;
            rt.anchoredPosition = new Vector2(currentX, y);

            // 보조선
            if (!isRest)
                ledgerHelper.GenerateLedgerLines(
                    index,
                    baselineY,
                    spacing,
                    currentX,
                    ledgerYOffset
                );

            // 다음 음표
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
