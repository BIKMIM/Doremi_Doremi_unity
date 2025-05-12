// 🎵 NoteSpawner.cs (BeamRenderer 포함 버전)
using UnityEngine;
using System.Collections.Generic;
using System;

public class NoteSpawner : MonoBehaviour
{
    [Header("🎼 Clefs")]
    [SerializeField] private GameObject clefTreblePrefab;
    [SerializeField] private GameObject clefBassPrefab;

    [Header("🖓 Time Signatures")]
    [SerializeField] private GameObject timeSig_2_4_Prefab;
    [SerializeField] private GameObject timeSig_3_4_Prefab;
    [SerializeField] private GameObject timeSig_4_4_Prefab;
    [SerializeField] private GameObject timeSig_3_8_Prefab;
    [SerializeField] private GameObject timeSig_4_8_Prefab;
    [SerializeField] private GameObject timeSig_6_8_Prefab;

    [Header("🔧 Settings")]
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

    [Header("🔧 Note Settings")]
    [SerializeField] private float staffHeight = 150f;
    [SerializeField] private float beatSpacingFactor = 2.0f;
    [SerializeField] private float noteYOffset = 0f;
    [SerializeField] private float noteScale = 2f;

    [Header("Dotted Note Settings")]
    [SerializeField] private float dottedNoteScale = 1.0f;

    [Header("🎼 Bar Line Settings")]
    [SerializeField] private GameObject barLinePrefab;
    [SerializeField] private float barLineWidth = 60f;
    [SerializeField] private float barLineHeight = 160f;
    [SerializeField] private float barLineVerticalOffset = -30f;

    private Dictionary<int, List<RectTransform>> beamGroups = new();
    private BeamRenderer beamRenderer;
    private NoteDataLoader dataLoader;
    private NoteMapper noteMapper;
    private LedgerLineHelper ledgerHelper;
    private KeySignatureRenderer keySigRenderer;

    private void Awake()
    {
        dataLoader = new NoteDataLoader(songsJson);
        noteMapper = new NoteMapper();
        ledgerHelper = new LedgerLineHelper(prefabProvider.LedgerLinePrefab, notesContainer, -2.1f);
        beamRenderer = new BeamRenderer(prefabProvider.BeamPrefab, notesContainer);
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
        var loader = new NoteDataLoader(songsJson);
        var songList = loader.LoadSongs();
        if (songList?.songs == null || selectedSongIndex >= songList.songs.Count) return;

        var song = songList.songs[selectedSongIndex];

        SpawnClef(song.clef);
        keySigRenderer.Render(song.key);
        SpawnTimeSignature(song.time);
        ClearNotes();

        // ── 여기에 마디선 복구 ──
        float spacing = staffHeight / 4f;
        float centerX = notesContainer.rect.width * 0.5f;
        float startX = -centerX + spacing * -18f;
        float baseY = Mathf.Round(notesContainer.anchoredPosition.y);
        SpawnBarLines(song, spacing, startX, baseY);

        // 음표 + 빔
        SpawnSongNotes(song, spacing, startX, baseY);
        foreach (var kvp in beamGroups)
        {
            Debug.Log($"[BeamDebug] rendering beam for ID={kvp.Key}, stems={kvp.Value.Count}");
            if (kvp.Value.Count < 2) continue;
            bool isAbove = kvp.Value[0].anchoredPosition.y > 0;
            beamRenderer.RenderBeam(kvp.Value, isAbove);
        }

    }



    private void SpawnClef(string clef)
    {
        var prefab = clef == "Bass" ? clefBassPrefab : clefTreblePrefab;
        if (!prefab) return;

        var obj = Instantiate(prefab, linesContainer);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = clef == "Bass" ? bassClefPosition : trebleClefPosition;
        rt.sizeDelta = clef == "Bass" ? bassClefSize : trebleClefSize;
    }

    private void SpawnTimeSignature(string time)
    {
        var prefab = time switch
        {
            "2/4" => timeSig_2_4_Prefab,
            "3/4" => timeSig_3_4_Prefab,
            "4/4" => timeSig_4_4_Prefab,
            "3/8" => timeSig_3_8_Prefab,
            "4/8" => timeSig_4_8_Prefab,
            "6/8" => timeSig_6_8_Prefab,
            _ => null
        };

        if (!prefab) return;
        var obj = Instantiate(prefab, linesContainer);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = timeSignaturePosition;
        rt.sizeDelta = new Vector2(timeSignatureWidth, staffHeight);
    }

    private void ClearNotes()
    {
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
            Destroy(notesContainer.GetChild(i).gameObject);
    }

    private void SpawnSongNotes(Song song, float spacing, float startX, float baseY)
    {

        float currentX = startX;
        float accumulatedBeats = 0f;

        foreach (var note in song.notes)
        {
            string pitch = note.pitch;
            string duration = note.duration;
            int beamId = note.beam;

            bool isRest = duration.EndsWith("R");
            bool isDotted = duration.Contains(".");
            string baseCode = duration.Replace("R", "").Replace(".", "");
            float beat = GetBeatLength(baseCode);
            if (isDotted) beat *= 1.5f;

            GameObject wrap = null; // ✅ 여기서 미리 선언

            if (!isRest && noteMapper.TryGetIndex(pitch, out float index))
            {
                float x = currentX;
                float y = baseY + index * spacing;

                GameObject flagPrefab = (beamId >= 0)
                    ? null
                    : GetFlagPrefab(baseCode);

                wrap = NoteFactory.CreateNoteWrap(
                    notesContainer,
                    prefabProvider.GetNoteHead(baseCode),
                    baseCode == "1" ? null : prefabProvider.NoteStemPrefab,
                    flagPrefab,
                    null,
                    index < 2.5f,
                    new Vector2(x, y),
                    noteScale,
                    spacing
                );
            }

            // ✅ 이 부분은 wrap이 null이 아닐 때만 실행
            if (beamId >= 0 && wrap != null)
            {
                var stem = wrap.transform.Find("Stem")?.GetComponent<RectTransform>();
                if (stem != null)
                {
                    Debug.Log($"[BeamDebug] adding stem for pitch={pitch}, beamId={beamId}");
                    if (!beamGroups.ContainsKey(beamId))
                        beamGroups[beamId] = new List<RectTransform>();
                    beamGroups[beamId].Add(stem);
                }
            }

            currentX += spacing * beatSpacingFactor * Mathf.Max(beat, 0.85f);
            accumulatedBeats += beat;
        }
    }

    /// <summary>
    /// 4/4 기준으로 전체 길이에 맞춰 자동으로 마디선을 그립니다.
    /// </summary>
    private void SpawnBarLines(Song song, float spacing, float startX, float baseY)
    {
        // 전체 박자 계산
        float totalBeats = 0f;
        foreach (var note in song.notes)
        {
            string dur = note.duration;
            string code = dur.Replace("R", "").Replace(".", "");
            float b = GetBeatLength(code);
            if (dur.Contains(".")) b *= 1.5f;
            totalBeats += b;
        }

        // 마디 수 (4/4 기준)
        int barCount = Mathf.FloorToInt(totalBeats / 4f);

        for (int i = 1; i <= barCount; i++)
        {
            float x = startX + i * 4f * spacing * beatSpacingFactor;
            DrawBarLineAtX(x, baseY);
        }
    }


    private void DrawBarLineAtX(float x, float baseY)
    {
        var bar = Instantiate(barLinePrefab, notesContainer);
        var rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, baseY + barLineVerticalOffset);
        rt.sizeDelta = new Vector2(barLineWidth, barLineHeight);
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

    private float GetBeatLength(string code) => code switch
    {
        "1" => 4f,
        "2" => 2f,
        "4" => 1f,
        "8" => 0.5f,
        "16" => 0.25f,
        _ => 1f
    };
}