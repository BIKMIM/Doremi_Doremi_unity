using UnityEngine;
using System.Collections.Generic;
using System;

public class NoteSpawner : MonoBehaviour
{
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
    [SerializeField] private float staffHeight = 150f;
    [SerializeField] private float beatSpacingFactor = 2.0f;
    [SerializeField] private float noteYOffset = 0f;
    [SerializeField] private float noteScale = 2f;

    [Header("🎯 Dotted Note Settings")]
    [SerializeField] private Vector2 dottedNoteOffsetRatio = new Vector2(0.45f, 0.3f);
    [SerializeField] private Vector2 dottedNoteOffsetAbsolute = new Vector2(0f, -20f);






    [SerializeField] private float dottedNoteScale = 1.0f;                              // 크기 배율



    private NoteDataLoader dataLoader;
    private NoteMapper noteMapper;
    private LedgerLineHelper ledgerHelper;
    private KeySignatureRenderer keySigRenderer;

    private void Awake()
    {
        dataLoader = new NoteDataLoader(songsJson);
        noteMapper = new NoteMapper();
        ledgerHelper = new LedgerLineHelper(prefabProvider.LedgerLinePrefab, notesContainer, yOffsetRatio: -2.1f);


        keySigRenderer = new KeySignatureRenderer(
     prefabProvider.SharpKeySignaturePrefab,
     prefabProvider.FlatKeySignaturePrefab,
     linesContainer,
     staffHeight / 4f,
     Mathf.Round(notesContainer.anchoredPosition.y)
 );

    }

    private void Start()
    {

        var test = Instantiate(prefabProvider.SharpKeySignaturePrefab, notesContainer);
        test.name = "TestSharpManual";
        test.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        var songList = dataLoader.LoadSongs();
        if (songList?.songs == null || songList.songs.Length == 0) return;
        if (selectedSongIndex < 0 || selectedSongIndex >= songList.songs.Length) return;

        var song = songList.songs[selectedSongIndex];

        SpawnClef(song.clef);
        keySigRenderer.Render(song.key);
        SpawnTimeSignature(song.time);
        ClearNotes();
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
        float baseY = Mathf.Round(notesContainer.anchoredPosition.y);
        float centerX = notesContainer.rect.width * 0.5f;
        // 조표 수를 감안해서 offset
        int keyAccidentalCount = KeySignatureHelper.GetAccidentals(song.key).Count;
        float keyOffsetX = keyAccidentalCount * spacing * 0.5f;

        // 기존 X 시작점 + 조표 간 거리만큼 밀어줌
        float currentX = -centerX + spacing * -18f + keyOffsetX;
        float verticalCorrection = spacing * -1.0f;



        foreach (var noteStr in song.notes)
        {
            string[] parts = noteStr.Split(':');
            if (parts.Length != 2) continue;

            string rawPitch = parts[0];
            string durationCode = parts[1];

            string pureDuration = durationCode.Replace("R", "").Replace(".", "");
            bool isRest = durationCode.EndsWith("R");
            bool isDotted = durationCode.Contains(".");

            if (!isRest)
            {
                if (!noteMapper.TryGetIndex(rawPitch, out float index))
                    continue;

                float x = currentX;
                float y = baseY + index * spacing + noteYOffset * spacing + verticalCorrection;

                GameObject wrap = NoteFactory.CreateNoteWrap(
                    notesContainer,
                    prefabProvider.GetNoteHead(pureDuration),
                    pureDuration == "1" ? null : prefabProvider.NoteStemPrefab,
                    GetFlagPrefab(pureDuration),
                    null,
                    index < 2.5f,
                    new Vector2(x, y),
                    noteScale,
                    spacing
                );

                if (isDotted)
                {
                    GameObject dot = UnityEngine.Object.Instantiate(prefabProvider.NoteDotPrefab, wrap.transform);
                    RectTransform rtDot = dot.GetComponent<RectTransform>();
                    rtDot.anchorMin = rtDot.anchorMax = new Vector2(0.5f, 0f); // 피벗에 맞춤
                    rtDot.pivot = new Vector2(0.5f, 0f);  // 기준점을 note-head 아래쪽에 맞춤

                    // 🎯 음표 헤드 기준 위치 계산
                    var noteHead = wrap.transform.Find("NoteHead")?.GetComponent<RectTransform>();
                    Vector2 headPos = noteHead != null ? noteHead.anchoredPosition : Vector2.zero;

                    // ✅ 오프셋: 오른쪽으로 30~40px, 위로 10~15px 정도 이동
                    Vector2 dotOffset = new Vector2(30f, 10f);  // 상황에 따라 이 값은 조정 가능

                    rtDot.anchoredPosition = headPos + dotOffset;
                    rtDot.localScale = Vector3.one * dottedNoteScale;
                }





                if (rawPitch.Contains("#") || rawPitch.Contains("b"))
                {
                    string accCode = rawPitch.Contains("#") ? "#" : "b";
                    GameObject accPrefab = prefabProvider.GetAccidental(accCode);
                    if (accPrefab != null)
                    {
                        var acc = Instantiate(accPrefab, wrap.transform);
                        var rt = acc.GetComponent<RectTransform>();
                        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(-spacing * 0.3f, verticalCorrection);
                        rt.localScale = Vector3.one;
                    }
                }

                ledgerHelper.GenerateLedgerLines(index, spacing, x, baseY, verticalCorrection);
            }

            currentX += spacing * beatSpacingFactor * GetBeatLength(pureDuration);
        }
    }

    private GameObject GetFlagPrefab(string code)
    {
        return code switch
        {
            "8" => prefabProvider.NoteFlag8Prefab,
            "16" => prefabProvider.NoteFlag16Prefab,
            _ => null
        };
    }

    private float GetBeatLength(string code)
    {
        return code switch
        {
            // 🎵 온음표
            "1" => 2f,
            "1." => 3f,     // 2 + 1
            "1R" => 2f,
            "1R." => 3f,

            // 🎵 2분음표
            "2" => 2f,
            "2." => 3f,
            "2R" => 2f,
            "2R." => 3f,

            // 🎵 4분음표
            "4" => 1.5f,
            "4." => 2.25f,
            "4R" => 1.5f,
            "4R." => 2.25f,

            // 🎵 8분음표
            "8" => 1f,
            "8." => 1.5f,
            "8R" => 1f,
            "8R." => 1.5f,

            // 🎵 16분음표
            "16" => 1f,
            "16." => 1.5f,
            "16R" => 1f,
            "16R." => 1.5f,

            // 기본값 (예외 처리)
            _ => 1f
        };
    }
}
