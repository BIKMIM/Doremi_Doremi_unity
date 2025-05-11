using UnityEngine;

/// <summary>
/// 악보 데이터를 기반으로 음표를 생성하는 컴포넌트
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [Header("Helpers")]
    [SerializeField] private NotePrefabProvider prefabProvider;

    [Header("🎹 UI")]
    [SerializeField] private RectTransform linesContainer;
    [SerializeField] private RectTransform notesContainer;

    [Header("📄 Data")]
    [SerializeField] private TextAsset songsJson;
    [SerializeField] private int selectedSongIndex = 0;

    [Header("⚙ Settings")]
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
                stemDown = code == "1" ? false : index >= -1f;
            }

            float y = baselineY + index * spacing + noteYOffset;

            if (!isRest)
            {
                ledgerHelper.GenerateLedgerLines(index, baselineY, spacing, currentX, ledgerYOffset);
            }

            if (code == "1")
            {
                y += wholeNoteYOffset;
                y += 20f; // 피벗 보정
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
                    null, // dotPrefab은 추후 구현
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