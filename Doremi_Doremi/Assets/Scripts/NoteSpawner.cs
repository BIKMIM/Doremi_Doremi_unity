using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// NoteSpawner.cs - 해상도 독립적 음표 생성 시스템
// 모든 크기와 위치를 비율 기반으로 계산하여 어떤 해상도에서도 동일한 비율로 표시

public class NoteSpawner : MonoBehaviour
{
    [Header("Json 파일 로딩 스크립트가 붙은 오브젝트")]
    public JsonLoader jLoader;

    [Header("노래 번호 - 0번이 첫번째 곡")]
    public int selectedSongIndex = 0;

    [Header("음표 배치 대상 패널")]
    public RectTransform staffPanel;

    [Header("🎼 음자리표 프리팹")]
    public GameObject trebleClefPrefab; // Clef-Treble 프리팹 연결
    public GameObject bassClefPrefab;   // Clef-Bass 프리팹 연결 (필요시)

    [Header("🎼 조표 프리팹")]
    public GameObject sharpPrefab;      // Sharp 프리팹 연결
    public GameObject flatPrefab;       // Flat 프리팹 연결

    [Header("음표 머리 프리팹")]
    public GameObject noteHeadPrefab;

    [Header("음표 조립 프리팹")]
    public NoteAssembler assembler;

    [Header("박자표 프리팹")]
    public GameObject timeSig2_4Prefab;
    public GameObject timeSig3_4Prefab;
    public GameObject timeSig3_8Prefab;
    public GameObject timeSig4_4Prefab;
    public GameObject timeSig4_8Prefab;
    public GameObject timeSig6_8Prefab;

    [Header("🎼 덧줄 프리팹")]
    public GameObject ledgerLinePrefab; // LedgerLine 프리팹 연결

    // 곡 로딩 후 파싱된 TimeSignature 객체
    private MusicLayoutConfig.TimeSignature currentSongTimeSignature;

    private Dictionary<string, float> noteIndexTable = new Dictionary<string, float>
{
    // 🎼 C3 옥타브 (인덱스 -13 ~ -7)
    { "C3", -13f},   { "C#3", -13f},   { "Db3", -13f},   // C3 = Db3
    { "D3", -12f},   { "D#3", -12f},   { "Eb3", -12f},   // D3
    { "E3", -11f},   { "E#3", -11f},   { "Fb3", -11f},   // E3 = Fb3
    { "F3", -10f},   { "F#3", -10f},   { "Gb3", -10f},   // F3
    { "G3", -9f},    { "G#3", -9f},    { "Ab3", -9f},    // G3
    { "A3", -8f},    { "A#3", -8f},    { "Bb3", -8f},    // A3
    { "B3", -7f},    { "B#3", -7f},    { "Cb3", -7f},    // B3 = Cb4

    // 🎼 C4 옥타브 (인덱스 -6 ~ 0)
    { "C4", -6f},    { "C#4", -6f},    { "Db4", -6f},    // C4
    { "D4", -5f},    { "D#4", -5f},    { "Eb4", -5f},    // D4
    { "E4", -4f},    { "E#4", -4f},    { "Fb4", -4f},    // E4
    { "F4", -3f},    { "F#4", -3f},    { "Gb4", -3f},    // F4
    { "G4", -2f},    { "G#4", -2f},    { "Ab4", -2f},    // G4
    { "A4", -1f},    { "A#4", -1f},    { "Bb4", -1f},    // A4
    { "B4", 0f},     { "B#4", 0f},     { "Cb4", 0f},     // B4 = Cb5

    // 🎼 C5 옥타브 (인덱스 1 ~ 7)
    { "C5", 1f},     { "C#5", 1f},     { "Db5", 1f},     // C5
    { "D5", 2f},     { "D#5", 2f},     { "Eb5", 2f},     // D5
    { "E5", 3f},     { "E#5", 3f},     { "Fb5", 3f},     // E5
    { "F5", 4f},     { "F#5", 4f},     { "Gb5", 4f},     // F5
    { "G5", 5f},     { "G#5", 5f},     { "Ab5", 5f},     // G5
    { "A5", 6f},     { "A#5", 6f},     { "Bb5", 6f},     // A5
    { "B5", 7f},     { "B#5", 7f},     { "Cb5", 7f},     // B5 = Cb6

    // 🎼 C6 옥타브 (인덱스 8 ~ 14)
    { "C6", 8f},     { "C#6", 8f},     { "Db6", 8f},     // C6
    { "D6", 9f},     { "D#6", 9f},     { "Eb6", 9f},     // D6
    { "E6", 10f},    { "E#6", 10f},    { "Fb6", 10f},    // E6
    { "F6", 11f},    { "F#6", 11f},    { "Gb6", 11f},    // F6
    { "G6", 12f},    { "G#6", 12f},    { "Ab6", 12f},    // G6
    { "A6", 13f},    { "A#6", 13f},    { "Bb6", 13f},    // A6
    { "B6", 14f},    { "B#6", 14f},    { "Cb6", 14f},    // B6 = Cb7

    // 🎼 추가 옥타브 (필요시 확장 가능)
    // C2 옥타브 (인덱스 -20 ~ -14)
    { "C2", -20f},   { "C#2", -20f},   { "Db2", -20f},   // C2
    { "D2", -19f},   { "D#2", -19f},   { "Eb2", -19f},   // D2
    { "E2", -18f},   { "E#2", -18f},   { "Fb2", -18f},   // E2
    { "F2", -17f},   { "F#2", -17f},   { "Gb2", -17f},   // F2
    { "G2", -16f},   { "G#2", -16f},   { "Ab2", -16f},   // G2
    { "A2", -15f},   { "A#2", -15f},   { "Bb2", -15f},   // A2
    { "B2", -14f},   { "B#2", -14f},   { "Cb2", -14f},   // B2 = Cb3

    // C7 옥타브 (인덱스 15 ~ 21)
    { "C7", 15f},    { "C#7", 15f},    { "Db7", 15f},    // C7
    { "D7", 16f},    { "D#7", 16f},    { "Eb7", 16f},    // D7
    { "E7", 17f},    { "E#7", 17f},    { "Fb7", 17f},    // E7
    { "F7", 18f},    { "F#7", 18f},    { "Gb7", 18f},    // F7
    { "G7", 19f},    { "G#7", 19f},    { "Ab7", 19f},    // G7
    { "A7", 20f},    { "A#7", 20f},    { "Bb7", 20f},    // A7
    { "B7", 21f},    { "B#7", 21f},    { "Cb7", 21f}     // B7 = Cb8
};

    // 🎼 조표 위치 정의 (높은음자리표 기준)
    private Dictionary<string, float> trebleKeySignaturePositions = new Dictionary<string, float>
    {
        // 샵 순서: F# C# G# D# A# E# B#
        { "F#", 4f },   // F5 위치
        { "C#", 1f },   // C5 위치  
        { "G#", 5f },   // G5 위치
        { "D#", 2f },   // D5 위치
        { "A#", -1f },   // A4 위치
        { "E#", 3f },   // E5 위치
        { "B#", 0f },   // B4 위치
        
        // 플랫 순서: Bb Eb Ab Db Gb Cb Fb
        { "Bb", 0f },   // B4 위치
        { "Eb", 3f },   // E5 위치
        { "Ab", -1f },   // A5 위치 
        { "Db", 2f },   // D5 위치
        { "Gb", -2f },   // G5 위치
        { "Cb", 1f },   // C5 위치
        { "Fb", -3f }    // F5 위치
    };

    // 🎼 조표 위치 정의 (낮은음자리표 기준) 
    private Dictionary<string, float> bassKeySignaturePositions = new Dictionary<string, float>
    {
        // 샵 순서: F# C# G# D# A# E# B# (낮은음자리표는 2도 아래)
        { "F#", 2f },   // D5 위치
        { "C#", -1f },  // A4 위치
        { "G#", 3f },   // E5 위치
        { "D#", 0f },   // B4 위치
        { "A#", 4f },   // F5 위치
        { "E#", 1f },   // C5 위치
        { "B#", -2f },  // G4 위치
        
        // 플랫 순서: Bb Eb Ab Db Gb Cb Fb (낮은음자리표는 2도 아래)
        { "Bb", -2f },  // G4 위치
        { "Eb", 1f },   // C5 위치
        { "Ab", 4f },   // F5 위치
        { "Db", 0f },   // B4 위치
        { "Gb", 3f },   // E5 위치
        { "Cb", -1f },  // A4 위치
        { "Fb", 2f }    // D5 위치
    };



    private HashSet<string> lineNotes = new HashSet<string>
{
    // 🎼 오선 5줄 (위에서부터)
    // F5 줄 (최상단 줄)
    "F5", "F#5", "Gb5", "E#5",
    // D5 줄 (4번째 줄)  
    "D5", "D#5", "Eb5",
    // B4 줄 (3번째 줄, 중앙)
    "B4", "B#4", "Cb5",
    // G4 줄 (2번째 줄)
    "G4", "G#4", "Ab4",
    // E4 줄 (최하단 줄)
    "E4", "E#4", "Fb4",

    // 🎼 오선 아래 덧줄들
    // C4 덧줄 (오선 바로 아래 첫 번째 덧줄)
    "C4", "C#4", "Db4", "B#3",
    // A3 덧줄 (두 번째 덧줄)  
    "A3", "A#3", "Bb3",
    // G3 덧줄 (세 번째 덧줄)
    "G3", "G#3", "Ab3",
    // E3 덧줄 (네 번째 덧줄)
    "E3", "E#3", "Fb3",
    // C3 덧줄 (다섯 번째 덧줄)
    "C3", "C#3", "Db3", "B#2",
    
    // 🎼 더 아래 덧줄들 (필요시)
    "A2", "A#2", "Bb2",  // 여섯 번째 덧줄
    "G2", "G#2", "Ab2",  // 일곱 번째 덧줄
    "E2", "E#2", "Fb2",  // 여덟 번째 덧줄
    "C2", "C#2", "Db2",  // 아홉 번째 덧줄

    // 🎼 오선 위 덧줄들
    // A5 덧줄 (오선 바로 위 첫 번째 덧줄)
    "A5", "A#5", "Bb5",
    // C6 덧줄 (두 번째 덧줄)
    "C6", "C#6", "Db6", "B#5",
    // E6 덧줄 (세 번째 덧줄)
    "E6", "E#6", "Fb6",
    // G6 덧줄 (네 번째 덧줄)
    "G6", "G#6", "Ab6",
    // B6 덧줄 (다섯 번째 덧줄)
    "B6", "B#6", "Cb7",
    
    // 🎼 더 위 덧줄들 (필요시)
    "D7", "D#7", "Eb7",  // 여섯 번째 덧줄
    "F7", "F#7", "Gb7",  // 일곱 번째 덧줄
    "A7", "A#7", "Bb7"   // 여덟 번째 덧줄
};

    void Start()
    {
        JsonLoader.SongList songList = jLoader.LoadSongs();

        if (songList == null || selectedSongIndex >= songList.songs.Count)
        {
            Debug.LogError("❌ 유효한 곡이 없습니다.");
            return;
        }
        


        JsonLoader.SongData song = songList.songs[selectedSongIndex];
        Debug.Log($"🎵 \"{song.title}\"의 음표 {song.notes.Count}개 생성 시작");

        this.currentSongTimeSignature = ParseTimeSignatureFromString(song.timeSignature);

        // 🎯 해상도 독립적 비율 기반 레이아웃
        LayoutCompleteScore(song);
    }

 


    // 🎼 조표 생성 함수 (해상도 독립적)
    private float SpawnKeySignature(float initialX, float staffSpacing, string keySignature, string clef)
    {
        if (string.IsNullOrEmpty(keySignature))
        {
            Debug.Log("🎼 조표 없음");
            return 0f;
        }

        // 조표 문자열 파싱 (예: \"F#,C#\" 또는 \"Bb,Eb\")
        string[] keySignatures = keySignature.Split(',')
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrEmpty(k))
            .ToArray();

        if (keySignatures.Length == 0)
        {
            Debug.Log("🎼 유효한 조표 없음");
            return 0f;
        }

        float currentX = initialX;
        float totalWidth = 0f;

        // 조표별 위치 맵 선택
        Dictionary<string, float> positions = clef.ToLower() == "bass" ? 
            bassKeySignaturePositions: trebleKeySignaturePositions;

        Debug.Log("🎼 조표 생성 시작: {keySignature} ({clef} 음자리표)");

        foreach (string key in keySignatures)
        {
            if (!positions.ContainsKey(key))
            {
                Debug.LogWarning("⚠️ 알 수 없는 조표: {key}");
                continue;
            }

            float noteIndex = positions[key];
            float width = SpawnSingleKeySignature(currentX, staffSpacing, key, noteIndex);

            currentX += width;
            totalWidth += width;
        }

        Debug.Log("🎼 조표 생성 완료: 총 너비={totalWidth:F1}");
        return totalWidth + staffSpacing * 0.3f; // 조표 후 약간의 여백
    }

    // 🎼 개별 조표 생성 함수 (해상도 독립적)
    private float SpawnSingleKeySignature(float x, float staffSpacing, string keySignature, float noteIndex)
    {
        bool isSharp = keySignature.Contains("#");
        bool isFlat = keySignature.Contains("b");

        GameObject prefabToUse = null;
        float symbolWidth = 0f;
        float symbolHeight = 0f;

        // 1. 먼저 크기 설정
        if (isSharp && sharpPrefab != null)
        {
            prefabToUse = sharpPrefab;
            symbolWidth = staffSpacing * 0.8f;  // 샵 가로
            symbolHeight = staffSpacing * 1.8f; // 샵 세로
        }
        else if (isFlat && flatPrefab != null)
        {
            prefabToUse = flatPrefab;
            symbolWidth = staffSpacing * 0.8f; // 플랫 가로
            symbolHeight = staffSpacing * 1.5f; // 플랫 세로
        }

        if (prefabToUse == null)
        {
            Debug.LogWarning($"⚠️ {keySignature} 조표 프리팹이 설정되지 않았습니다.");
            return staffSpacing * 0.5f; // 기본 너비 반환
        }

        // 2. 객체 생성
        GameObject keySignatureInstance = Instantiate(prefabToUse, staffPanel);
        RectTransform keyRT = keySignatureInstance.GetComponent<RectTransform>();

        // 3. 크기 설정
        keyRT.sizeDelta = new Vector2(symbolWidth, symbolHeight);

        // 4. 앵커와 피벗 설정
        keyRT.anchorMin = new Vector2(0.5f, 0.5f);
        keyRT.anchorMax = new Vector2(0.5f, 0.5f);
        keyRT.pivot = new Vector2(0.5f, 0.5f);

        // 5. 위치 계산 (크기 계산 후에)
        float posX = x + symbolWidth * 0.5f;
        float posY = noteIndex * staffSpacing * 0.5f;

        // 6. 플랫 Y 오프셋 적용
        if (isFlat)
        {
            posY += staffSpacing * 0.3f; // 오선 간격의 10%만큼 위로 이동
        }

        // 7. 최종 위치 설정
        keyRT.anchoredPosition = new Vector2(posX, posY);

        Debug.Log($"   → {keySignature}: 크기={symbolWidth:F1}x{symbolHeight:F1}, 위치=({posX:F1}, {posY:F1})");

        return symbolWidth + staffSpacing * -0.2f; // 조표 간 간격
    }




    // NoteSpawner.cs의 LayoutCompleteScore 함수 완전 수정
    private void LayoutCompleteScore(JsonLoader.SongData song)
    {
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        

        // 🎯 StaffPanel 기준 해상도 독립적 레이아웃
        float panelWidth = staffPanel.rect.width;
        float panelHeight = staffPanel.rect.height;

        // 패널의 실제 좌표 범위 계산
        float leftEdge = -panelWidth * 0.5f;   // 패널 왼쪽 끝
        float rightEdge = panelWidth * 0.5f;   // 패널 오른쪽 끝

        float leftMargin = panelWidth * 0.02f; // 패널 너비의 2% 여백
        float usableWidth = panelWidth * 0.96f; // 패널 너비의 96% 사용

        // 음자리표를 패널 맨 왼쪽에서 시작
        float startX = leftEdge + leftMargin;
        float currentX = startX;  // ← currentX를 여기서 먼저 선언

        Debug.Log($"🎯 패널 기준 레이아웃: 패널너비={panelWidth:F1}, 왼쪽끝={leftEdge:F1}, 시작X={startX:F1}");

        // 1. 🎼 음자리표 생성 (패널 맨 왼쪽)
        float clefWidth = SpawnClef(currentX, spacing, song.clef);
        currentX += clefWidth;

        // 2. 🎼 조표 생성 (음자리표 바로 옆)
        float keySignatureWidth = SpawnKeySignature(currentX, spacing, song.keySignature, song.clef);
        currentX += keySignatureWidth;

        // 3. 🎵 박자표 생성 (음자리표 바로 옆)
        float timeSignatureWidth = SpawnTimeSignatureSymbol(currentX, spacing);
        currentX += timeSignatureWidth;

        // 4. 🎶 음표들 배치 (남은 공간에 균등 배치)
        float usedWidth = clefWidth + timeSignatureWidth;
        float remainingWidth = usableWidth - usedWidth - (startX - leftEdge); // 실제 남은 공간
        float noteSpacing = remainingWidth / song.notes.Count;

        Debug.Log($"🎯 완전한 레이아웃: 음자리표={clefWidth:F1}, 박자표={timeSignatureWidth:F1}, 남은공간={remainingWidth:F1}, 음표간격={noteSpacing:F1}");

        int order = 0;
        foreach (string rawNote in song.notes)
        {
            NoteData note = NoteParser.Parse(rawNote);

            if (note.isRest)
            {
                SpawnRestAtPosition(currentX, noteSpacing, spacing, note);
            }
            else
            {
                SpawnNoteAtPosition(currentX, noteSpacing, spacing, note, order);
            }

            currentX += noteSpacing;
            order++;
        }

        Debug.Log($"✅ 패널 기준 악보 완료: {song.clef} 음자리표 + 박자표 + {order}개 음표");
    }


    // SpawnClef 함수 수정 (해상도 독립적)
    private float SpawnClef(float initialX, float staffSpacing, string clefType)
    {
        GameObject clefPrefab = null;

        if (string.IsNullOrEmpty(clefType))
        {
            clefType = "treble";
        }

        switch (clefType.ToLower())
        {
            case "treble":
                clefPrefab = trebleClefPrefab;
                break;
            case "bass":
                clefPrefab = bassClefPrefab;
                break;
            default:
                Debug.LogWarning($"⚠️ 알 수 없는 음자리표 타입: {clefType}. treble을 사용합니다.");
                clefPrefab = trebleClefPrefab;
                break;
        }

        if (clefPrefab == null)
        {
            Debug.LogWarning($"⚠️ {clefType} 음자리표 프리팹이 설정되지 않았습니다.");
            return staffSpacing * 2f;
        }

        GameObject clefInstance = Instantiate(clefPrefab, staffPanel);
        RectTransform clefRT = clefInstance.GetComponent<RectTransform>();

        // 🎯 완전히 해상도 독립적 크기 설정 (패널 높이 기준)
        float panelHeight = staffPanel.rect.height;
        float desiredHeight = panelHeight * 0.7f; // 패널 높이의 40%
        float desiredWidth;
        float yOffset = 0f; // ← 이 변수 선언이 중요!

        // 음자리표별 비율
        if (clefType.ToLower() == "treble")
        {
            // Treble: 크고 좁게
            desiredHeight = panelHeight * 0.7f;  // 높은음자리표 높이
            desiredWidth = desiredHeight * 0.3f;  // 높은음자리표 넓이
        }
        else if (clefType.ToLower() == "bass")
        {
            // Bass: 작고 넓게  
            desiredHeight = panelHeight * 0.35f;  // 낮은음자리표 높이
            desiredWidth = desiredHeight * 0.6f;  // 낮은음자리표 넓이
            yOffset = panelHeight * 0.05f; // ← 이 부분이 추가됨
        }
        else
        {
            desiredWidth = desiredHeight * 0.375f; // 기본 비율
        }

        // 크기 적용
        clefRT.sizeDelta = new Vector2(desiredWidth, desiredHeight);

        // 앵커와 피벗 설정
        clefRT.anchorMin = new Vector2(0.5f, 0.5f);
        clefRT.anchorMax = new Vector2(0.5f, 0.5f);
        clefRT.pivot = new Vector2(0.5f, 0.5f);

        // 위치 설정 (음자리표의 중심이 해당 X 좌표에 오도록)
        float posX = initialX + desiredWidth * 0.5f;
        clefRT.anchoredPosition = new Vector2(posX, yOffset);

        Debug.Log($"🎼 {clefType} 음자리표 (패널기준): 크기={desiredWidth:F1}x{desiredHeight:F1}, 위치=({posX:F1}, 0)");

        return desiredWidth + staffSpacing * 0.2f;
    }



    
    private void SpawnRestAtPosition(float x, float noteSpacing, float spacing, NoteData note)
    {
        float restY = spacing * 0.0f;
        Vector2 restPos = new Vector2(x + noteSpacing * 0.5f, restY);

        Debug.Log($"🎵 쉼표 생성: {note.duration}분 쉼표 at X={restPos.x:F1}");

        assembler.SpawnRestNote(restPos, note.duration, note.isDotted);
    }




    // SpawnTimeSignatureSymbol 함수 수정 (해상도 독립적)
    private float SpawnTimeSignatureSymbol(float initialX, float staffSpacing)
    {
        GameObject prefabToUse = GetTimeSignaturePrefab();

        if (prefabToUse == null)
        {
            Debug.LogError($"박자표 프리팹을 찾을 수 없습니다: {currentSongTimeSignature.beatsPerMeasure}/{currentSongTimeSignature.beatUnitType}");
            return staffSpacing * 1.5f;
        }

        GameObject timeSigInstance = Instantiate(prefabToUse, staffPanel);
        RectTransform tsRT = timeSigInstance.GetComponent<RectTransform>();

        // 🎯 완전히 해상도 독립적 크기 설정 (패널 높이 기준)
        float panelHeight = staffPanel.rect.height;
        float desiredHeight = panelHeight * 0.4f; // 패널 높이의 30%
        float desiredWidth = desiredHeight * 0.4f; // 세로로 긴 비율

        tsRT.sizeDelta = new Vector2(desiredWidth, desiredHeight);

        // 앵커와 피벗 설정
        tsRT.anchorMin = new Vector2(0.5f, 0.5f);
        tsRT.anchorMax = new Vector2(0.5f, 0.5f);
        tsRT.pivot = new Vector2(0.5f, 0.5f);

        // 위치 설정
        float posX = initialX + desiredWidth * 0.5f;
        tsRT.anchoredPosition = new Vector2(posX, 0f);

        Debug.Log($"🎵 박자표 (패널기준): 크기={desiredWidth:F1}x{desiredHeight:F1}, 위치=({posX:F1}, 0)");

        return desiredWidth + staffSpacing * 0.5f;
    }



    // 덧줄이 필요한 음표들과 덧줄 위치 정의
    // NoteSpawner.cs의 C3~B6 완전한 덧줄 시스템

    // 🎼 음표 인덱스 참조 (B4 = 0 기준)
    // C3(-13), D3(-12), E3(-11), F3(-10), G3(-9), A3(-8), B3(-7)
    // C4(-6), D4(-5), E4(-4), F4(-3), G4(-2), A4(-1), B4(0)
    // C5(1), D5(2), E5(3), F5(4), G5(5), A5(6), B5(7)  
    // C6(8), D6(9), E6(10), F6(11), G6(12), A6(13), B6(14)

    // 🎼 오선 위치 (덧줄이 필요하지 않은 음표들)
    // E4(-4=-2*2), G4(-2=-1*2), B4(0=0*2), D5(2=1*2), F5(4=2*2)

    // 🎼 덧줄 위치 (오선 바깥 음표들)
    // 아래 덧줄: -6(-3*2), -10(-5*2), -14(-7*2), -18(-9*2), -22(-11*2), -26(-13*2)
    // 위 덧줄: 6(3*2), 10(5*2), 14(7*2), 18(9*2), 22(11*2), 26(13*2)



    // 🎼 해상도 독립적 덧줄 생성 함수
    // 🎼 정확한 덧줄 규칙이 적용된 SpawnLedgerLines 함수
    private void SpawnLedgerLines(Vector2 notePosition, string noteName, float staffSpacing)
    {
        if (!noteIndexTable.ContainsKey(noteName))
        {
            Debug.LogWarning($"⚠️ 알 수 없는 음표: {noteName}");
            return;
        }

        if (ledgerLinePrefab == null)
        {
            Debug.LogWarning("⚠️ 덧줄 프리팹이 설정되지 않았습니다.");
            return;
        }

        float noteIndex = noteIndexTable[noteName];

        Debug.Log($"🎼 {noteName} 음표: 인덱스={noteIndex}, Y위치={notePosition.y:F1}");

        // 🎯 덧줄이 필요한 음표인지 확인
        // 오선 범위: E4(-4) ~ F5(4), 즉 -4 ~ 4 사이는 덧줄 불필요
        if (noteIndex >= -4f && noteIndex <= 4f)
        {
            Debug.Log($"🎼 {noteName}: 오선 내부 음표, 덧줄 불필요");
            return;
        }

        // 🎯 정확한 악보 덧줄 규칙 적용
        List<float> ledgerPositions = new List<float>();

        if (noteIndex < -4f) // 오선 아래
        {
            Debug.Log($"🎼 {noteName}: 오선 아래 음표");

            // 🎯 음표가 짝수 인덱스(덧줄 위)인지 홀수 인덱스(덧줄 사이)인지 확인
            bool isOnLedgerLine = (Mathf.RoundToInt(noteIndex) % 2 == 0);

            if (isOnLedgerLine)
            {
                // 음표가 덧줄 위에 있는 경우: 해당 덧줄부터 위쪽 모든 덧줄
                Debug.Log($"🎼 {noteName}: 덧줄 위에 위치 (인덱스={noteIndex})");
                for (float ledgerPos = noteIndex; ledgerPos <= -6f; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
            else
            {
                // 음표가 덧줄 사이에 있는 경우: 위쪽 덧줄만
                float upperLedger = Mathf.Ceil(noteIndex / 2f) * 2f; // 위쪽 가장 가까운 짝수
                Debug.Log($"🎼 {noteName}: 덧줄 사이에 위치 (인덱스={noteIndex}), 위쪽 덧줄={upperLedger}");
                for (float ledgerPos = upperLedger; ledgerPos <= -6f; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
        }
        else if (noteIndex > 4f) // 오선 위
        {
            Debug.Log($"🎼 {noteName}: 오선 위 음표");

            bool isOnLedgerLine = (Mathf.RoundToInt(noteIndex) % 2 == 0);

            if (isOnLedgerLine)
            {
                // 음표가 덧줄 위에 있는 경우: 6부터 해당 덧줄까지
                Debug.Log($"🎼 {noteName}: 덧줄 위에 위치 (인덱스={noteIndex})");
                for (float ledgerPos = 6f; ledgerPos <= noteIndex; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
            else
            {
                // 음표가 덧줄 사이에 있는 경우: 아래쪽 덧줄부터
                float lowerLedger = Mathf.Floor(noteIndex / 2f) * 2f; // 아래쪽 가장 가까운 짝수
                Debug.Log($"🎼 {noteName}: 덧줄 사이에 위치 (인덱스={noteIndex}), 아래쪽 덧줄={lowerLedger}");
                for (float ledgerPos = 6f; ledgerPos <= lowerLedger; ledgerPos += 2f)
                {
                    ledgerPositions.Add(ledgerPos);
                }
            }
        }

        Debug.Log($"🎼 {noteName}에 대해 {ledgerPositions.Count}개 덧줄 생성: [{string.Join(", ", ledgerPositions)}]");

        // 🎯 덧줄 생성
        foreach (float ledgerIndex in ledgerPositions)
        {
            CreateSingleLedgerLine(notePosition.x, ledgerIndex, staffSpacing);
        }
    }



    // 🎼 개별 덧줄 생성 함수 (해상도 독립적)
    private void CreateSingleLedgerLine(float x, float ledgerIndex, float staffSpacing)
    {
        GameObject ledgerLine = Instantiate(ledgerLinePrefab, staffPanel);
        RectTransform ledgerRT = ledgerLine.GetComponent<RectTransform>();

        // 🎯 완전히 해상도 독립적 크기 설정 (패널 기준 비율)
        float panelHeight = staffPanel.rect.height;
        float ledgerWidth = staffSpacing * 1.6f;  // 오선 간격의 1.6배 (적절한 크기)
        float ledgerThickness = MusicLayoutConfig.GetLineThickness(staffPanel); // 오선과 동일한 두께

        ledgerRT.sizeDelta = new Vector2(ledgerWidth, ledgerThickness);

        // 🎯 해상도 독립적 앵커와 피벗 설정
        ledgerRT.anchorMin = new Vector2(0.5f, 0.5f);
        ledgerRT.anchorMax = new Vector2(0.5f, 0.5f);
        ledgerRT.pivot = new Vector2(0.5f, 0.5f);

        // 🎯 위치 설정 (ledgerIndex에 따른 Y 좌표)
        float ledgerY = ledgerIndex * staffSpacing * 0.5f;
        ledgerRT.anchoredPosition = new Vector2(x, ledgerY);

        // 🎨 덧줄 스타일 설정 (오선과 동일하게)
        UnityEngine.UI.Image ledgerImage = ledgerLine.GetComponent<UnityEngine.UI.Image>();
        if (ledgerImage != null)
        {
            ledgerImage.color = Color.black;
        }

        Debug.Log($"   → 덧줄: 인덱스={ledgerIndex}, Y={ledgerY:F1}, 크기={ledgerWidth:F1}x{ledgerThickness:F1}");
    }




    // SpawnNoteAtPosition 함수 수정 (덧줄 추가)
    private void SpawnNoteAtPosition(float x, float noteSpacing, float spacing, NoteData note, int order)
    {
        if (!noteIndexTable.ContainsKey(note.noteName))
        {
            Debug.LogWarning($"🎵 알 수 없는 음표 이름: {note.noteName}");
            return;
        }

        float noteIndex = noteIndexTable[note.noteName];
        float y = noteIndex * spacing * 0.5f;

        // 음표를 할당된 공간의 중앙에 배치
        Vector2 pos = new Vector2(x + noteSpacing * 0.5f, y);

        // 🎼 덧줄 먼저 생성 (음표 아래 레이어에 표시되도록)
        SpawnLedgerLines(pos, note.noteName, spacing);

        bool isOnLine = lineNotes.Contains(note.noteName);

        Debug.Log($"🎵 음표 생성: {note.noteName} at X={pos.x:F1}, Y={pos.y:F1}");

        // 음표 생성 (덧줄 위에 표시됨)
        if (note.isDotted)
        {
            assembler.SpawnDottedNoteFull(pos, noteIndex, isOnLine, note.duration);
        }
        else
        {
            assembler.SpawnNoteFull(pos, noteIndex, note.duration);
        }
    }



    private GameObject GetTimeSignaturePrefab()
    {
        string tsKey = $"{currentSongTimeSignature.beatsPerMeasure}/{currentSongTimeSignature.beatUnitType}";

        return tsKey switch
        {
            "2/4" => timeSig2_4Prefab,
            "3/4" => timeSig3_4Prefab,
            "4/4" => timeSig4_4Prefab,
            "3/8" => timeSig3_8Prefab,
            "4/8" => timeSig4_8Prefab,
            "6/8" => timeSig6_8Prefab,
            _ => timeSig4_4Prefab // 기본값
        };
    }

    private MusicLayoutConfig.TimeSignature ParseTimeSignatureFromString(string tsString)
    {
        if (string.IsNullOrEmpty(tsString) || !tsString.Contains("/"))
        {
            Debug.LogWarning($"잘못된 박자표 문자열입니다: {tsString}. 기본값(4/4)을 사용합니다.");
            return new MusicLayoutConfig.TimeSignature(4, 4);
        }

        string[] parts = tsString.Split('/');
        if (parts.Length == 2 && int.TryParse(parts[0], out int beats) && int.TryParse(parts[1], out int unitType))
        {
            return new MusicLayoutConfig.TimeSignature(beats, unitType);
        }

        Debug.LogWarning($"박자표 문자열 파싱에 실패했습니다: {tsString}. 기본값(4/4)을 사용합니다.");
        return new MusicLayoutConfig.TimeSignature(4, 4);
    }
}