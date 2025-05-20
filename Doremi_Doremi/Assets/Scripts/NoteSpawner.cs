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

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel); // 음표 간격 계산.
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio; // 음표 머리 너비 계산.

        float currentX = 0f; // 현재 X 좌표 초기화.
        int order = 0; // 음표 순서 초기화.

        foreach (string rawNote in song.notes)    
        {
            NoteData note = NoteParser.Parse(rawNote); // 🎯 새 구조로 파싱

            // 쉼표 처리
            if (note.isRest)
            {
                float restY = spacing * 0.0f; // 🎯 오선 중간보다 살짝 위

                float spacingX = MusicLayoutConfig.GetBeatSpacingFor(staffPanel, note.duration, note.isDotted);
                Vector2 restPos = new Vector2(currentX + spacingX * 0.5f, restY); // 🎯 살짝 오른쪽으로 이동

                assembler.SpawnRestNote(restPos, note.duration, note.isDotted);
                currentX += spacingX; // 🎯 생성 후 위치 증가

                order++;
                continue;
            }

            // 유효한 음인지 확인
            if (!noteIndexTable.ContainsKey(note.noteName))
            {
                Debug.LogWarning($"🎵 알 수 없는 음표 이름: {note.noteName}");
                continue;
            }

            float noteIndex = noteIndexTable[note.noteName]; 
            float y = noteIndex * spacing * 0.5f; 
            Vector2 pos = new Vector2(currentX, y); 

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

            currentX += MusicLayoutConfig.GetBeatSpacingFor(staffPanel, note.duration, note.isDotted);
            order++;
        }

        Debug.Log($"✅ \"{song.title}\"의 음표 {order}개 생성 완료");
    }
}

