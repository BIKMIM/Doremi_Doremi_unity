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



    private Dictionary<string, int> noteIndexTable = new Dictionary<string, int> // 딕셔너리 변수 선언. 음 이름에 따라 줄 인덱스를 정의한 매핑 테이블
    {
        { "C3", -13}, { "D3", -12}, { "E3", -11}, { "F3", -10 }, { "G3", -9 }, { "A3", -8 }, { "B3", -7 },
        { "C4", -6 }, { "D4", -5 }, { "E4", -4 }, { "F4", -3  }, { "G4", -2 }, { "A4", -1 }, { "B4",  0 },  //세번째 줄 시(B4)가 기준점 0 임.
        { "C5",  1 }, { "D5",  2 }, { "E5",  3 }, { "F5",  4  }, { "G5",  5 }, { "A5",  6 }, { "B5",  7 },
        { "C6",  8 }, { "D6",  9 }, { "E6", 10 }, { "F6", 11  }, { "G6", 12 }, { "A6", 13 }, { "B6", 14 }
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

        float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
        float headWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio;
        float horizontalGap = headWidth * 3f;

        int order = 0;

        foreach (string noteName in song.notes)
        {
            if (!noteIndexTable.ContainsKey(noteName))
            {
                Debug.LogWarning($"🎵 알 수 없는 음표 이름: {noteName}");
                continue;
            }

            int index = noteIndexTable[noteName];
            float y = index * spacing * 0.5f;
            float x = order * horizontalGap;

            assembler.SpawnNoteFull(new Vector2(x, y));
            order++;

            Debug.Log($"🎵 \"{song.title}\"의 음표 {song.notes.Count}개 생성 완료");

        }
    }
}

