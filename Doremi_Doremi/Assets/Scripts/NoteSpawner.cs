using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        { "C3", -13f}, { "D3", -12f}, { "E3", -11f}, { "F3", -10f }, { "G3", -9f }, { "A3", -8f }, { "B3", -7f },
        { "C4", -6f }, { "D4", -5f }, { "E4", -4f }, { "F4", -3f  }, { "G4", -2f }, { "A4", -1f }, { "B4",  0f },
        { "C5",  1f }, { "D5",  2f }, { "E5",  3f }, { "F5",  4f  }, { "G5",  5f }, { "A5",  6f }, { "B5",  7f },
        { "C6",  8f }, { "D6",  9f }, { "E6", 10f }, { "F6", 11f  }, { "G6", 12f }, { "A6", 13f }, { "B6", 14f }
    };

    private HashSet<string> lineNotes = new HashSet<string>
    {
        "E4", "G4", "B4", "D5", "F5", // 오선 5줄
        "C4", "A3", "G3", "E3",       // 오선 아래 덧줄
        "A5", "C6", "E6", "G6"        // 오선 위 덧줄
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
        float currentX = startX;

        Debug.Log($"🎯 패널 기준 레이아웃: 패널너비={panelWidth:F1}, 왼쪽끝={leftEdge:F1}, 시작X={startX:F1}");

        // 1. 🎼 음자리표 생성 (패널 맨 왼쪽)
        float clefWidth = SpawnClef(currentX, spacing, song.clef);
        currentX += clefWidth;

        // 2. 🎵 박자표 생성 (음자리표 바로 옆)
        float timeSignatureWidth = SpawnTimeSignatureSymbol(currentX, spacing);
        currentX += timeSignatureWidth;

        // 3. 🎶 음표들 배치 (남은 공간에 균등 배치)
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
            desiredWidth = desiredHeight * 0.55f;  // 낮은음자리표 넓이
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