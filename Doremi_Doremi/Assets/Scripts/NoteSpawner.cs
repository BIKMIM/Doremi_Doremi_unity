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
    [SerializeField] private float barLineWidth = 60f;   // 마디선 너비
    [SerializeField] private float barLineHeight = 160f; // 마디선 높이 (staffHeight와 동일)
    [SerializeField] private float barLineVerticalOffset = -30f;


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
        int keyAccidentalCount = KeySignatureHelper.GetAccidentals(song.key).Count; // KeySignatureHelper가 정의되어 있다고 가정
        float keyOffsetX = keyAccidentalCount * spacing * 0.5f;
        float startX = -centerX + spacing * -18f + keyOffsetX; // spacing * -18f 값은 시작 여백 조정 값으로 보임
        float baseY = Mathf.Round(notesContainer.anchoredPosition.y);

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
        for (int i = notesContainer.childCount - 1; i >= 0; i--)
            Destroy(notesContainer.GetChild(i).gameObject);
    }

    private void SpawnSongNotes(Song song, float spacing, float startX, float baseY)
    {
        float currentX = startX;
        float verticalCorrection = spacing * -1.0f;
        float accumulatedBeats = 0f;

        float beatsPerMeasure = GetBeatsPerMeasure(song.time); // 각 마디의 총 비트 수

        foreach (var noteStr in song.notes)
        {
            string[] parts = noteStr.Split(':');
            if (parts.Length != 2) continue;

            string rawPitch = parts[0];
            string durationCode = parts[1];

            bool isDotted = durationCode.Contains(".");
            bool isRest = rawPitch == "R";

            string baseCode = durationCode.Replace("R", "").Replace(".", "");
            float baseBeat = GetBeatLength(baseCode); // 실제 음악적 길이 (4분음표 = 1f 기준)
            float beat = isDotted ? baseBeat * 1.5f : baseBeat;

            // SpawnSongNotes 메서드 내 visualBeat 계산 부분
            float visualBeat = isRest
                ? baseCode switch
                {
                    "1" => 1.0f,   // 온쉼표 (예시 값, 실제 프리팹 너비에 맞게 조정)
                    "2" => 0.8f,   // 2분쉼표 (예시 값)
                    "4" => 0.7f,   // 4분쉼표 (예시 값)
                    "8" => 0.6f,   // 8분쉼표 (기존 0.5f에서 증가시켜 테스트)
                    "16" => 0.5f,  // 16분쉼표 (예시 값)
                    _ => 0.6f
                }
                : Mathf.Max(beat, 0.85f); // 음표는 최소 너비 0.85f (4분음표보다 약간 넓게)
            // ▲▲▲ visualBeat 계산 완료 ▲▲▲

            // 마디 경계 체크 (beatsPerMeasure 사용)
            int barIndexBefore = Mathf.FloorToInt(accumulatedBeats / beatsPerMeasure);
            int barIndexAfter = Mathf.FloorToInt((accumulatedBeats + beat) / beatsPerMeasure);

            if (barIndexAfter > barIndexBefore)
            {
                float nextBarBeatPosition = (barIndexBefore + 1) * beatsPerMeasure;
                float fractionOfBeatBeforeBarLine = (nextBarBeatPosition - accumulatedBeats) / beat;
                fractionOfBeatBeforeBarLine = Mathf.Clamp01(fractionOfBeatBeforeBarLine);

                // 현재 음표/쉼표의 전체 시각적 너비
                float currentNoteVisualWidth = spacing * beatSpacingFactor * visualBeat;

                // 마디선 X 위치 계산
                float barLineX = currentX + (currentNoteVisualWidth * fractionOfBeatBeforeBarLine);
                DrawBarLineAtX(barLineX, baseY);

                // 원래 코드에 있던 마디선 후 currentX 강제 조정 부분은 일단 제거하고,
                // 각 음표/쉼표의 visualBeat에 따른 currentX 전진에 의존합니다.
                // 필요하다면 여기에 마디선 너비만큼의 추가적인 간격 조정을 고려할 수 있습니다.
                // 예: currentX = barLineX + (barLineWidth / 2f) + smallPadding; (이후 음표가 마디선 바로 뒤에서 시작하도록)
                // 하지만, 이렇게 하면 tied note(붙임줄 음표) 같은 복잡한 경우 처리가 어려워질 수 있습니다.
                // 우선은 각 음표의 visualBeat로 간격을 조절하는 것을 기본으로 합니다.
            }

            // 음표 또는 쉼표 렌더링
            if (isRest)
            {
                GameObject restPrefab = prefabProvider.GetRest(baseCode);
                if (restPrefab != null)
                {
                    GameObject restNote = Instantiate(restPrefab, notesContainer);
                    RectTransform rt = restNote.GetComponent<RectTransform>();
                    rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);

                    float restYOffset = baseCode switch
                    {
                        "1" => spacing * 1.0f,  // 온쉼표 (네 번째 칸 아래에 붙도록)
                        "2" => spacing * 0.5f,  // 2분쉼표 (세 번째 칸에 걸치도록 - 필요시 조정) / 기존 0.8f에서 수정
                        _ => 0f // 다른 쉼표들은 보통 중앙선 근처
                    };
                    // 쉼표 Y 위치 조정 (restYOffset은 baseY를 기준으로 한 상대적 위치)
                    rt.anchoredPosition = new Vector2(currentX, baseY + verticalCorrection + restYOffset + (noteYOffset * spacing));
                    rt.localScale = Vector3.one * noteScale;
                }
            }
            else // 음표 렌더링
            {
                if (!noteMapper.TryGetIndex(rawPitch, out float index)) continue;

                float notePosX = currentX;
                float notePosY = baseY + index * spacing + noteYOffset * spacing + verticalCorrection;

                GameObject wrap = NoteFactory.CreateNoteWrap( // NoteFactory가 정의되어 있다고 가정
                    notesContainer,
                    prefabProvider.GetNoteHead(baseCode),
                    baseCode == "1" ? null : prefabProvider.NoteStemPrefab, // 온음표는 기둥 없음
                    GetFlagPrefab(baseCode), // 8분, 16분 음표 등의 꼬리
                    null, // Augmentation dot (여기서는 isDotted로 따로 처리)
                    index < 2.5f, // Stem direction (true면 위로, false면 아래로 - 대략 중앙 B 기준)
                    new Vector2(notePosX, notePosY),
                    noteScale,
                    spacing
                );

                if (isDotted)
                {
                    GameObject dot = Instantiate(prefabProvider.NoteDotPrefab, wrap.transform); // 점은 음표 머리(wrap)의 자식으로
                    RectTransform rtDot = dot.GetComponent<RectTransform>();

                    // 점 위치 설정 (음표 머리 오른쪽 옆)
                    var noteHeadRect = wrap.transform.Find("NoteHead")?.GetComponent<RectTransform>();
                    float dotXOffset = (noteHeadRect != null ? noteHeadRect.sizeDelta.x * noteScale / 2f : spacing * 0.3f) + (spacing * 0.1f); // 머리 너비의 절반 + 약간의 간격
                    float dotYOffset = 0; // 기본적으로 머리와 같은 Y축. 필요시 조정 (예: 칸에 있으면 칸 오른쪽에, 줄에 있으면 줄 바로 위 칸)

                    // 홀수 인덱스(줄에 걸친 음표)면 점을 살짝 위로 (오른쪽 옆 칸에 찍히도록)
                    if (index % 2 != 0)
                    { // index가 0, 1, 2, 3, 4... 로 줄/칸을 나타낸다고 가정
                        // dotYOffset = spacing / 2f; // 예시: 한 칸의 절반만큼 위로
                    }

                    rtDot.anchorMin = new Vector2(0.5f, 0.5f); // 머리 기준 중앙
                    rtDot.anchorMax = new Vector2(0.5f, 0.5f);
                    rtDot.pivot = new Vector2(0.5f, 0.5f); // 자기 자신의 중앙
                    rtDot.anchoredPosition = new Vector2(dotXOffset, dotYOffset); // 계산된 오프셋 적용
                    rtDot.localScale = Vector3.one * dottedNoteScale; // Inspector에서 설정한 점 크기 배율 사용
                }

                ledgerHelper.GenerateLedgerLines(index, spacing, currentX, baseY, verticalCorrection);
            }

            // 다음 음표 위치를 위해 currentX 업데이트
            currentX += spacing * beatSpacingFactor * visualBeat;
            accumulatedBeats += beat;
        }
    }

    private float GetBeatsPerMeasure(string timeSignature)
    {
        // 박자표에 따른 한 마디의 총 비트 수 (4분음표=1비트 기준)
        return timeSignature switch
        {
            "2/4" => 2f,
            "3/4" => 3f,
            "4/4" => 4f,
            "3/8" => 1.5f, // 8분음표 3개 = 1.5개의 4분음표
            "4/8" => 2f,   // 8분음표 4개 = 2개의 4분음표
            "6/8" => 3f,   // 8분음표 6개 = 3개의 4분음표 (보통 2박으로 느껴짐)
            _ => 4f      // 인식할 수 없는 박자표면 기본 4/4로 처리
        };
    }

    private void DrawBarLineAtX(float x, float baseY)
    {
        GameObject barLine = Instantiate(barLinePrefab, notesContainer);
        RectTransform rt = barLine.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0f, 0.5f); // 왼쪽 중앙 기준
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); // 자기 자신의 중심을 기준으로 위치 설정

        rt.anchoredPosition = new Vector2(x, baseY + barLineVerticalOffset);
        rt.sizeDelta = new Vector2(barLineWidth, barLineHeight);
        rt.localScale = Vector3.one;
    }

    private GameObject GetFlagPrefab(string code)
    {
        return code switch
        {
            "8" => prefabProvider.NoteFlag8Prefab,
            "16" => prefabProvider.NoteFlag16Prefab,
            // "32" => prefabProvider.NoteFlag32Prefab, // 필요시 추가
            _ => null
        };
    }

    private float GetBeatLength(string code)
    {
        // 음표/쉼표 코드에 따른 음악적 길이 (4분음표 = 1f)
        return code switch
        {
            "1" => 4f,   // 온음표
            "2" => 2f,   // 2분음표
            "4" => 1f,   // 4분음표
            "8" => 0.5f, // 8분음표
            "16" => 0.25f,// 16분음표
            // "32" => 0.125f, // 필요시 추가
            _ => 1f      // 모르는 코드면 기본 4분음표 길이
        };
    }
}