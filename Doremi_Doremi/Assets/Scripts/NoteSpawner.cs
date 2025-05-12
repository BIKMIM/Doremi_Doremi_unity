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
    [SerializeField] private Vector2 dottedNoteOffsetRatio = new Vector2(0.45f, 0.3f);  // 아래로 이동
    [SerializeField] private Vector2 dottedNoteOffsetAbsolute = new Vector2(0f, -20f);  // 미세 보정

    [Header("🎼 Bar Line Settings")]
    [SerializeField] private GameObject barLinePrefab;  // 마디선 프리팹
    [SerializeField] private float barLineWidth = 2f;   // 마디선 너비
    [SerializeField] private float barLineHeight = 150f; // 마디선 높이 (staffHeight와 동일)

    [SerializeField] private float dottedNoteScale = 1.0f;  // 점음표 크기 배율


    private NoteDataLoader dataLoader;
    private NoteMapper noteMapper;
    private LedgerLineHelper ledgerHelper;
    private KeySignatureRenderer keySigRenderer;

    private void Awake()
    {
        // 데이터 로더와 도우미 객체 초기화
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
        var songList = dataLoader.LoadSongs();
        if (songList?.songs == null || songList.songs.Length == 0) return;
        if (selectedSongIndex < 0 || selectedSongIndex >= songList.songs.Length) return;

        var song = songList.songs[selectedSongIndex];

        // 음악의 구성 요소 생성
        SpawnClef(song.clef);
        keySigRenderer.Render(song.key);
        SpawnTimeSignature(song.time);
        ClearNotes();

        // ✔ 마디선 관련 계산
        float spacing = staffHeight / 4f;
        float centerX = notesContainer.rect.width * 0.5f;
        int keyAccidentalCount = KeySignatureHelper.GetAccidentals(song.key).Count;
        float keyOffsetX = keyAccidentalCount * spacing * 0.5f;
        float startX = -centerX + spacing * -18f + keyOffsetX;
        float baseY = Mathf.Round(notesContainer.anchoredPosition.y);

        // 마디선 생성
        SpawnBarLines(song, spacing, startX, baseY);

        // 음표 생성
        SpawnSongNotes(song, spacing, startX, baseY);
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
        // 기존 음표 삭제
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
            Destroy(notesContainer.GetChild(i).gameObject);
    }

    private void SpawnSongNotes(Song song, float spacing, float startX, float baseY)
    {
        float currentX = startX;
        float verticalCorrection = spacing * -1.0f;

        float accumulatedBeats = 0f;
        float nextBarBeat = 4f;

        foreach (var noteStr in song.notes)
        {
            string[] parts = noteStr.Split(':');
            if (parts.Length != 2) continue;

            string rawPitch = parts[0];
            string durationCode = parts[1];

            bool isDotted = durationCode.Contains(".");
            bool isRest = durationCode.EndsWith("R");

            // 🔍 순수 박자 코드 (ex. "4." → "4")
            string baseCode = durationCode.Replace("R", "").Replace(".", "");
            float baseBeat = GetBeatLength(baseCode);
            float beat = isDotted ? baseBeat * 1.5f : baseBeat;

            // 🧮 마디선 위치 계산 (정확한 위치에 1줄만!)
            if (accumulatedBeats < nextBarBeat && accumulatedBeats + beat >= nextBarBeat)
            {
                float fraction = (nextBarBeat - accumulatedBeats) / beat;
                float barLineX = currentX + spacing * beatSpacingFactor * beat * fraction;

                DrawBarLineAtX(barLineX, baseY);
                nextBarBeat += 4f;
            }

            // 🎵 음표 생성
            if (!isRest)
            {
                if (!noteMapper.TryGetIndex(rawPitch, out float index)) continue;

                float x = currentX;
                float y = baseY + index * spacing + noteYOffset * spacing + verticalCorrection;

                GameObject wrap = NoteFactory.CreateNoteWrap(
                    notesContainer,
                    prefabProvider.GetNoteHead(baseCode),
                    baseCode == "1" ? null : prefabProvider.NoteStemPrefab,
                    GetFlagPrefab(baseCode),
                    null,
                    index < 2.5f,
                    new Vector2(x, y),
                    noteScale,
                    spacing
                );

                // 🎯 점음표 렌더링
                if (isDotted)
                {
                    GameObject dot = UnityEngine.Object.Instantiate(prefabProvider.NoteDotPrefab, wrap.transform);
                    RectTransform rtDot = dot.GetComponent<RectTransform>();
                    rtDot.anchorMin = rtDot.anchorMax = new Vector2(0.5f, 0f);
                    rtDot.pivot = new Vector2(0.5f, 0f);

                    var noteHead = wrap.transform.Find("NoteHead")?.GetComponent<RectTransform>();
                    Vector2 headPos = noteHead != null ? noteHead.anchoredPosition : Vector2.zero;

                    Vector2 dotOffset = new Vector2(30f, 10f); // 필요 시 조정
                    rtDot.anchoredPosition = headPos + dotOffset;
                    rtDot.localScale = Vector3.one * dottedNoteScale;
                }

                ledgerHelper.GenerateLedgerLines(index, spacing, x, baseY, verticalCorrection);
            }

            // 🧭 다음 음표 위치로 이동 (정확한 beat 기준, 1번만 이동)
            currentX += spacing * beatSpacingFactor * beat;
            accumulatedBeats += beat;
        }
    }


    private void DrawBarLineAtX(float x, float baseY)
    {
        GameObject barLine = Instantiate(barLinePrefab, notesContainer);
        RectTransform rt = barLine.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, baseY);
        rt.sizeDelta = new Vector2(barLineWidth, barLineHeight);
        rt.localScale = Vector3.one;
    }

    private void SpawnBarLines(Song song, float spacing, float startX, float baseY)
    {
        float totalBeats = 0f;
        foreach (var noteStr in song.notes)
        {
            string[] parts = noteStr.Split(':');
            if (parts.Length != 2) continue;

            string durationCode = parts[1];
            string pureDuration = durationCode.Replace("R", "").Replace(".", "");
            totalBeats += GetBeatLength(pureDuration); // 소수 허용
        }

        // 4/4 기준으로 마디선 추가
        int barCount = Mathf.FloorToInt(totalBeats / 4f);  // 4/4 기준

        for (int i = 1; i <= barCount; i++)
        {
            float x = startX + i * 4f * spacing * beatSpacingFactor;

            GameObject barLine = Instantiate(barLinePrefab, notesContainer);
            RectTransform rt = barLine.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, baseY);
            rt.sizeDelta = new Vector2(barLineWidth, barLineHeight);
            rt.localScale = Vector3.one;
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
            "1" => 4f,
            "2" => 2f,
            "4" => 1f,
            "8" => 0.5f,
            "16" => 0.25f,
            _ => 1f
        };
    }

}
