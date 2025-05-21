using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// NoteSpawner.cs - 음표 생성, 점음표 점위치 조정,
// JSON → NoteData 배열로 변환 후 음표 생성
// 음이름에 따라 줄위치 인덱스 컨트롤  , 음끼리 겹치지 않도록 위치 조정.

public class NoteSpawner : MonoBehaviour
{
    [Header("Json 파일 로딩 스크립트가 붙은 오브젝트")]
    public JsonLoader jLoader;

    [Header("노래 번호 - 0번이 첫번째 곡")]
    public int selectedSongIndex = 0;

    [Header("음표 배치 대상 패널")]
    public RectTransform staffPanel; // 패널 변수 선언.

    [Header("음표 머리 프리팹")]
    public GameObject noteHeadPrefab; // 프리팹 변수 선언.

    [Header("음표 조립 프리팹")]
    public NoteAssembler assembler; // NoteAssembler 스크립트 변수 선언. 음표 머리 생성하는 스크립트.

    [Header("박자표 프리팹")]
    public GameObject timeSig2_4Prefab;
    public GameObject timeSig3_4Prefab;
    public GameObject timeSig3_8Prefab;
    public GameObject timeSig4_4Prefab;
    public GameObject timeSig4_8Prefab;
    public GameObject timeSig6_8Prefab;

    // 곡 로딩 후 파싱된 TimeSignature 객체
    private MusicLayoutConfig.TimeSignature currentSongTimeSignature; // 여기에 현재 곡의 박자 정보를 저장


    private Dictionary<string, float> noteIndexTable = new Dictionary<string, float> // 딕셔너리 변수 선언. 음 이름에 따라 줄 인덱스를 정의한 매핑 테이블
    {
        { "C3", -13f}, { "D3", -12f}, { "E3", -11f}, { "F3", -10f }, { "G3", -9f }, { "A3", -8f }, { "B3", -7f },
        { "C4", -6f }, { "D4", -5f }, { "E4", -4f }, { "F4", -3f  }, { "G4", -2f }, { "A4", -1f }, { "B4",  0f },  //세번째 줄 시(B4)가 기준점 0 임.
        { "C5",  1f }, { "D5",  2f }, { "E5",  3f }, { "F5",  4f  }, { "G5",  5f }, { "A5",  6f }, { "B5",  7f },
        { "C6",  8f }, { "D6",  9f }, { "E6", 10f }, { "F6", 11f  }, { "G6", 12f }, { "A6", 13f }, { "B6", 14f }
    }; // 음 이름과 줄 인덱스 매핑 테이블. C3는 -13, B4는 0, C5는 1, D5는 2, E5는 3, F5는 4, G5는 5, A5는 6, B5는 7로 정의됨.


    private HashSet<string> lineNotes = new HashSet<string>
{
    // 오선 안쪽 (Treble Clef 기준 5줄)
    "E4", // 1번 줄 (맨 아래줄)
    "G4", // 2번 줄
    "B4", // 3번 줄
    "D5", // 4번 줄
    "F5", // 5번 줄 (맨 윗줄)

    // 오선 아래 덧줄
    "C4", // 첫 번째 덧줄 아래
    "A3", // 첫 번째 덧줄 아래
    "G3",
    "E3",

    // 오선 위 덧줄
    "A5",
    "C6",
    "E6", // 필요 시 더 확장 가능
    "G6"
};

    void Start()
    {

        JsonLoader.SongList songList = jLoader.LoadSongs(); // JsonLoader에서 노래 목록 로드.

        if (songList == null || selectedSongIndex >= songList.songs.Count) // 유효성 검사.
        {
            Debug.LogError("❌ 유효한 곡이 없습니다.");
            return;
        }

        JsonLoader.SongData song = songList.songs[selectedSongIndex]; // 선택한 곡 로드.
        Debug.Log($"🎵 \"{song.title}\"의 음표 {song.notes.Count}개 생성 시작");

        // 1. Json에서 읽어온 timeSignature 문자열을 MusicLayoutConfig.TimeSignature 구조체로 변환
        //    (이 변환 로직은 JsonLoader나 NoteSpawner에 추가해야 합니다)
        //    예: "4/4" -> new MusicLayoutConfig.TimeSignature(4, 4)
        this.currentSongTimeSignature = ParseTimeSignatureFromString(song.timeSignature); // ParseTimeSignatureFromString 함수는 직접 구현 필요

        // ⬇️ 이 부분 추가! (예시: 2마디를 화면에 표시한다고 가정)
        float measureVisualWidth = staffPanel.rect.width / 2f;

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel); // 음표 간격 계산.
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio; // 음표 머리 너비 계산.

        float currentX = 0f; // 현재 X 좌표 초기화.
        int order = 0; // 음표 순서 초기화.

        // --- 박자표 생성 및 배치 ---
        currentX = SpawnTimeSignatureSymbol(currentX, spacing);
        // --------------------------

        // 음표 생성

        foreach (string rawNote in song.notes)
        {
            Debug.Log($"Processing raw note: {rawNote}");
            NoteData note = NoteParser.Parse(rawNote);
            Debug.Log($"Parsed: {note.ToString()}, Duration: {note.duration}, IsRest: {note.isRest}, IsDotted: {note.isDotted}");

            // 새로운 방식으로 음표/쉼표의 시각적 너비 계산
            float noteWidth = MusicLayoutConfig.GetNoteVisualWidth(
                measureVisualWidth,             // 위에서 정의한 한 마디의 시각적 너비
                this.currentSongTimeSignature,  // 파싱된 현재 곡의 박자
                note.duration,
                note.isDotted
            );

            // 쉼표 처리
            if (note.isRest)
            {
                float restY = spacing * 0.0f;
                // 쉼표 위치는 currentX를 기준으로 하고, noteWidth를 고려하여 중앙 정렬 등을 할 수 있습니다.
                // 예: 현재 위치에서 쉼표 너비의 절반만큼 이동하여 중앙에 배치
                Vector2 restPos = new Vector2(currentX + noteWidth * 0.5f, restY);
                Debug.Log($"Attempting to spawn REST: {rawNote} at X: {restPos.x} with Width: {noteWidth}");
                assembler.SpawnRestNote(restPos, note.duration, note.isDotted);
                currentX += noteWidth; // 계산된 너비만큼 currentX 증가
            }
            else // 음표인 경우
            {
                if (!noteIndexTable.ContainsKey(note.noteName))
                {
                    Debug.LogWarning($"🎵 알 수 없는 음표 이름: {note.noteName}");
                    order++; // 누락된 음표도 순서는 증가시켜야 전체 카운트가 맞습니다.
                    continue;
                }

                float noteIndex = noteIndexTable[note.noteName];
                float y = noteIndex * spacing * 0.5f;
                // 음표 위치는 현재 currentX 값입니다. 음표 자체의 너비는 noteWidth로 표현됩니다.
                Vector2 pos = new Vector2(currentX, y);
                Debug.Log($"Attempting to spawn NOTE: {rawNote} at X: {pos.x} with Width: {noteWidth}");
                
                bool isOnLine = lineNotes.Contains(note.noteName);


                if (note.isDotted)
                {
                    assembler.SpawnDottedNoteFull(pos, noteIndex, isOnLine, note.duration);
                }
                else
                {
                    assembler.SpawnNoteFull(pos, noteIndex, note.duration);
                }
                Debug.Log($"🎵 음표: {note.noteName} | 길이: {note.duration}분음표 | 점음표: {note.isDotted}");
                currentX += noteWidth; // 계산된 너비만큼 currentX 증가
            }
            Debug.Log($"currentX after {rawNote}: {currentX}");
            order++;
        }
        Debug.Log($"✅ \"{song.title}\"의 음표 {order}개 생성 완료. 최종 currentX: {currentX}"); //
    }


    // 박자표 심볼을 생성하는 메서드
    private float SpawnTimeSignatureSymbol(float initialX, float staffSpacing)
    {
        GameObject prefabToUse = null;
        string tsKey = $"{this.currentSongTimeSignature.beatsPerMeasure}/{this.currentSongTimeSignature.beatUnitType}";

        switch (tsKey)
        {
            case "2/4": prefabToUse = timeSig2_4Prefab; break;
            case "3/4": prefabToUse = timeSig3_4Prefab; break;
            case "4/4": prefabToUse = timeSig4_4Prefab; break;
            case "3/8": prefabToUse = timeSig3_8Prefab; break;
            case "4/8": prefabToUse = timeSig4_8Prefab; break;
            case "6/8": prefabToUse = timeSig6_8Prefab; break;
            default:
                Debug.LogWarning($"지원하지 않는 박자표 프리팹 키: {tsKey}. 기본(4/4) 프리팹을 시도합니다.");
                prefabToUse = timeSig4_4Prefab; // 기본값 또는 null 처리 후 생성 안 함
                break;
        }

        if (prefabToUse == null)
        {
            Debug.LogError($"박자표 프리팹을 찾을 수 없습니다: {tsKey}");
            return initialX; // 박자표 없이 원래 X 위치 반환
        }

        GameObject timeSigInstance = Instantiate(prefabToUse, staffPanel);
        RectTransform tsRT = timeSigInstance.GetComponent<RectTransform>();

        // 크기 설정: 박자표의 전체 높이가 오선 4칸을 차지하도록 설정 (MusicLayoutConfig의 상수 활용)
        float desiredTotalHeight = staffSpacing * MusicLayoutConfig.TimeSignatureVerticalCoverage;

        // 프리팹의 원래 비율 유지를 위한 계산 (프리팹의 Pivot과 내부 구성에 따라 미세 조정 필요)
        // 간단히는, 프리팹 자체가 숫자 2개를 위아래로 포함하고, 그 전체의 RectTransform이라고 가정합니다.
        // 그리고 프리팹의 숫자 이미지는 부모 RectTransform 크기에 맞춰 늘어나도록 설정되어 있다고 가정합니다. (예: Stretch 모드)
        float originalPrefabWidth = tsRT.rect.width; // 또는 tsRT.sizeDelta.x (프리팹 설정에 따라)
        float originalPrefabHeight = tsRT.rect.height; // 또는 tsRT.sizeDelta.y

        float scaleFactor = 1f;
        if (originalPrefabHeight > 0) // 0으로 나누기 방지
        {
            scaleFactor = desiredTotalHeight / originalPrefabHeight;
        }

        float desiredTotalWidth = originalPrefabWidth * scaleFactor;
        tsRT.sizeDelta = new Vector2(desiredTotalWidth, desiredTotalHeight); // 크기 설정

        // 위치 설정
        // Y 위치: 오선 중앙. 박자표의 Pivot이 (0.5, 0.5)이고, 오선 중앙이 Y=0이라고 가정.
        float timeSigPosY = 0f;

        // X 위치: 약간의 왼쪽 여백 후 박자표 너비의 절반만큼 이동하여 중심 배치
        float leftPadding = staffSpacing * 1.5f; // 예: 오선 1.5칸 정도의 왼쪽 여백
        tsRT.anchoredPosition = new Vector2(initialX + leftPadding + desiredTotalWidth * 0.5f, timeSigPosY);

        // 다음 요소가 시작될 X 위치 반환 (박자표 오른쪽 약간의 여백 포함)
        return initialX + leftPadding + desiredTotalWidth + staffSpacing; // 오른쪽 여백으로 staffSpacing 하나 추가
    }


    // timeSignature 문자열을 파싱하는 헬퍼 함수 (예시)
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

