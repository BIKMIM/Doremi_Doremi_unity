using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;



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
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;
        float horizontalGap = headWidth * 3f; // 음표 머리 간격 계산. 머리 크기 * 3배.

        int order = 0; // 음표 순서 변수 초기화.

        foreach (string noteNameRaw in song.notes)
        {
            bool isDotted = noteNameRaw.EndsWith("."); // 음 이름이 점으로 끝나는지 확인.
            string pureNoteName = isDotted ? noteNameRaw.TrimEnd('.') : noteNameRaw; // 점을 제거한 순수 음 이름.


            if (!noteIndexTable.ContainsKey(pureNoteName)) // 음 이름이 매핑 테이블에 존재하는지 확인.
            {
                Debug.LogWarning($"🎵 알 수 없는 음표 이름: {pureNoteName}");
                continue;
            }


            float index = noteIndexTable[pureNoteName]; // 음 이름에 해당하는 줄 인덱스 가져오기.
            float y = index * spacing * 0.5f; // 줄 인덱스에 따라 y 좌표 계산. 0.5배로 조정.
            float x = order * horizontalGap; // 음표 순서에 따라 x 좌표 계산.



            if (isDotted) 
            {
            bool isOnLine = lineNotes.Contains(pureNoteName); 
                float dotY = isOnLine ? spacing * 0.3f : spacing * -0.2f;
                assembler.SpawnDottedNoteFull(new Vector2(x, y), index, isOnLine); // y는 음표 위치
            }

            else 
            {
            assembler.SpawnNoteFull(new Vector2(x, y));

            Debug.Log($"🎵 음표: {noteNameRaw} | 점음표: {isDotted}");
            }
            order++; 
        }

        // ✅ 루프 끝나고 총 갯수 출력
        Debug.Log($"✅ \"{song.title}\"의 음표 {order}개 생성 완료");
    }
}

