using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [SerializeField] private RectTransform ledgerContainer;

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

    [Header("🎼 Clef Settings")]
    [SerializeField] private Vector2 trebleClefPosition = new Vector2(30f, -115f);
    [SerializeField] private Vector2 trebleClefSize = new Vector2(140f, 280f);
    [SerializeField] private Vector2 bassClefPosition = new Vector2(30f, -115f);
    [SerializeField] private Vector2 bassClefSize = new Vector2(140f, 280f);

    [Header("Helpers")]
    [SerializeField] private NotePrefabProvider prefabProvider;

    [Header("📋 UI Targets")]
    [SerializeField] private RectTransform linesContainer;
    [SerializeField] private RectTransform notesContainer;

    [Header("📂 JSON Data")]
    [SerializeField] private TextAsset songsJson;
    [SerializeField] private int selectedSongIndex = 0;

    [Header("Settings")]
    [SerializeField] private float noteHeadOffsetRatio = -1.0f;  // spacing 대비 음표 머리 offset
    [SerializeField] private float ledgerYOffsetRatio = -0.5f;  // spacing 대비 덧줄 offset
    [SerializeField] private float staffHeight = 150f;
    [SerializeField] private float beatSpacingFactor = 2.0f;
    [SerializeField] private float noteScale = 2f;

    private NoteDataLoader dataLoader;
    private NoteMapper noteMapper;
    private LedgerLineHelper ledgerHelper; // 주석 해제

    private void Awake()
    {
        dataLoader = new NoteDataLoader(songsJson);
        noteMapper = new NoteMapper();
        // 보조선은 staff 라인이 그려진 linesContainer 좌표계에서 찍혀야 합니다.
        ledgerHelper = new LedgerLineHelper(prefabProvider.ledgerLinePrefab, ledgerContainer);
    }

    private void Start()
    {
        var songList = dataLoader.LoadSongs();
        if (songList?.songs == null || songList.songs.Length == 0) return;
        if (selectedSongIndex < 0 || selectedSongIndex >= songList.songs.Length) return;

        var song = songList.songs[selectedSongIndex];

        SpawnClef(song.clef);
        SpawnTimeSignature(song.time);
        ClearNotes();
        ClearLedgerLines();
        SpawnSongNotes(song);
    }

    private void SpawnClef(string clefType)
    {
        GameObject clefPrefab = clefType == "Bass" ? clefBassPrefab : clefTreblePrefab;
        if (!clefPrefab) return;

        var clef = Instantiate(clefPrefab, linesContainer);
        var rt = clef.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = clefType == "Bass" ? bassClefPosition : trebleClefPosition;
        rt.sizeDelta = clefType == "Bass" ? bassClefSize : trebleClefSize;
    }

    private void ClearLedgerLines()
    {
        for (int i = ledgerContainer.childCount - 1; i >= 0; i--)
            Destroy(ledgerContainer.GetChild(i).gameObject);
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

        if (prefab == null) return;
        var obj = Instantiate(prefab, linesContainer);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = timeSignaturePosition;
        rt.sizeDelta = new Vector2(timeSignatureWidth, staffHeight);
    }

    private void ClearNotes()
    {
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
            Destroy(notesContainer.GetChild(i).gameObject);
    }


    private void SpawnSongNotes(Song song)
    {
        float spacing = staffHeight / 4f;
        float baseY = 0f;
        float centerX = notesContainer.rect.width * 0.5f;
        float currentX = -centerX + spacing * -18f;

        // 음표에 적용될 기본 수직 보정값
        float noteVerticalCorrection = noteHeadOffsetRatio * spacing;

        // 덧줄에 적용될 최종 수직 보정값: (음표의 기본 보정값 + 덧줄 고유의 오프셋)
        float ledgerFinalVerticalCorrection = noteVerticalCorrection + (ledgerYOffsetRatio * spacing);

        foreach (var noteStr in song.notes)
        {
            var parts = noteStr.Split(':');
            if (parts.Length != 2) { continue; }
            string pitch = parts[0];
            string duration = parts[1];
            bool isRest = duration.EndsWith("R");
            string pureCode = isRest ? duration.Replace("R", "") : duration;
            float beatLength = GetBeatLength(pureCode);

            if (!isRest)
            {
                if (!noteMapper.TryGetIndex(pitch, out float index))
                {
                    currentX += spacing * beatSpacingFactor * beatLength;
                    continue;
                }

                float x = currentX;
                // 음표의 Y 위치 계산 시에는 noteVerticalCorrection 사용
                float y = baseY + index * spacing + noteVerticalCorrection;

                GameObject head = prefabProvider.GetNoteHead(pureCode);
                GameObject stem = (pureCode == "1") ? null : prefabProvider.noteStemPrefab;
                GameObject flag = pureCode switch
                {
                    "8" => prefabProvider.noteFlag8Prefab,
                    "16" => prefabProvider.noteFlag16Prefab,
                    _ => null
                };
                bool stemDown = index < 2.5f;

                NoteFactory.CreateNoteWrap(
                    notesContainer,
                    head, stem, flag, null,
                    stemDown,
                    new Vector2(x, y),
                    noteScale,
                    spacing
                );

                // 5) 덧줄 생성 시에는 ledgerFinalVerticalCorrection 사용
                ledgerHelper.GenerateLedgerLines(index, spacing, x, baseY, ledgerFinalVerticalCorrection); // <--- 수정된 부분
            }

            currentX += spacing * beatSpacingFactor * beatLength;
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