using UnityEngine;
using Object = UnityEngine.Object;

public class NoteSpawner : MonoBehaviour
{
    [Header("🎼 Clefs")]
    [SerializeField] private GameObject clefTreblePrefab;
    [SerializeField] private GameObject clefBassPrefab;

    [Header("🕓 Time Signatures")]
    [SerializeField] private GameObject timeSig2_4Prefab;
    [SerializeField] private GameObject timeSig3_4Prefab;
    [SerializeField] private GameObject timeSig4_4Prefab;
    [SerializeField] private GameObject timeSig3_8Prefab;
    [SerializeField] private GameObject timeSig4_8Prefab;
    [SerializeField] private GameObject timeSig6_8Prefab;

    [Header("♯♭ Key Signatures")]
    [SerializeField] private GameObject sharpPrefab;
    [SerializeField] private GameObject flatPrefab;

    [Header("Clef Settings")]
    [SerializeField] private Vector2 trebleClefPosition = new(30f, -115f);
    [SerializeField] private Vector2 trebleClefSize = new(140f, 280f);
    [SerializeField] private Vector2 bassClefPosition = new(30f, -115f);
    [SerializeField] private Vector2 bassClefSize = new(140f, 280f);

    [Header("Helpers")]
    [SerializeField] private NotePrefabProvider prefabProvider;

    [Header("UI")]
    [SerializeField] private RectTransform linesContainer;
    [SerializeField] private RectTransform notesContainer;

    [Header("Data")]
    [SerializeField] private TextAsset songsJson;
    [SerializeField] private int selectedSongIndex = 0;

    [Header("Settings")]
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
        if (songList?.songs == null || songList.songs.Length == 0)
        {
            Debug.LogError("[NoteSpawner] songs 배열이 비어 있거나 JSON 파싱 실패");
            return;
        }

        if (selectedSongIndex < 0 || selectedSongIndex >= songList.songs.Length)
        {
            Debug.LogError($"[NoteSpawner] selectedSongIndex ({selectedSongIndex}) 가 songs 배열 범위 초과");
            return;
        }

        var song = songList.songs[selectedSongIndex];

        // 🎼 Clef
        var clefRenderer = new ClefRenderer(
            clefTreblePrefab, clefBassPrefab, linesContainer,
            trebleClefPosition, trebleClefSize, bassClefPosition, bassClefSize
        );
        clefRenderer.Spawn(song.clef);

        // ♯♭ Key Signature
        var keyRenderer = new KeySignatureRenderer(
            linesContainer, sharpPrefab, flatPrefab,
            new Vector2(80f, 0f), 10f
        );
        keyRenderer.Spawn(song.key);

        // 🕓 Time Signature
        var timePrefabs = new TimeSignaturePrefabs
        {
            time2_4Prefab = timeSig2_4Prefab,
            time3_4Prefab = timeSig3_4Prefab,
            time4_4Prefab = timeSig4_4Prefab,
            time3_8Prefab = timeSig3_8Prefab,
            time4_8Prefab = timeSig4_8Prefab,
            time6_8Prefab = timeSig6_8Prefab,
        };
        var timeRenderer = new TimeSignatureRenderer(linesContainer, timePrefabs);
        timeRenderer.Spawn(song.time);

        // 🎵 Note rendering
        NoteUtility.Clear(notesContainer);  // 🧹 기존 음표 제거
        var noteRenderer = new NoteRenderer(
            prefabProvider, notesContainer, noteMapper, ledgerHelper,
            staffHeight, beatSpacing, noteYOffset, ledgerYOffset,
            noteScale, wholeNoteYOffset
        );
        var baselineY = linesContainer.GetChild(2).GetComponent<RectTransform>().anchoredPosition.y;
        noteRenderer.SpawnNotes(song, baselineY);
    }
}
