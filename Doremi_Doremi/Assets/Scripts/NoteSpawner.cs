using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class NoteSpawner : MonoBehaviour
{
    [Header ("Json 파일 로딩 스크립트가 붙은 오브젝트")]
    public JsonLoader jLoader;

    [Header ("노래 번호 - 0번이 첫번째 곡")]
    public int selectedSongIndex = 0;

    [Header("음표 배치 대상 패널")]  // 헤더는 제목. 헤더 아래에 변수 선언.
    public RectTransform staffPanel; // 패널 변수 선언.

    [Header("음표 머리 프리팹")]  
    public GameObject noteHeadPrefab; // 프리팹 변수 선언.

    private Dictionary<string, int> noteIndex = new Dictionary<string, int> // 딕셔너리 변수 선언. 음 이름에 따라 줄 인덱스를 정의한 매핑 테이블
    {
        { "C3", -13}, { "D3", -12}, { "E3", -11}, { "F3", -10 }, { "G3", -9 }, { "A3", -8 }, { "B3", -7 }, 
        { "C4", -6 }, { "D4", -5 }, { "E4", -4 }, { "F4", -3  }, { "G4", -2 }, { "A4", -1 }, { "B4",  0 },  //세번째 줄 시(B4)가 기준점 0 임.
        { "C5",  1 }, { "D5",  2 }, { "E5",  3 }, { "F5",  4  }, { "G5",  5 }, { "A5",  6 }, { "B5",  7 },  
        { "C6",  8 }, { "D6",  9 }, { "E6", 10 }, { "F6", 11  }, { "G6", 12 }, { "A6", 13 }, { "B6", 14 }
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

        foreach (string noteName in song.notes)
        {
            SpawnNoteHeadFromName(noteName);
        }

        Debug.Log($"🎵 \"{song.title}\"의 음표 {song.notes.Count}개 생성 완료");
    }

    public void SpawnNoteHead(float y) // 음표 머리 생성.
    {
        float staffPanelHeight = staffPanel.rect.height; // 패널 높이 가져오기.
        float spacing = MusicLayoutConfig.GetSpacing(staffPanel); // 각 줄 사이의 간격 계산.  

        float noteHeadWidth = spacing * MusicLayoutConfig.NoteHeadWidthRatio; // 음표 머리 너비 계산.
        float noteHeadHeight = spacing * MusicLayoutConfig.NoteHeadHeightRatio; // 음표 머리 높이 계산.

        GameObject noteHead = Instantiate(noteHeadPrefab, staffPanel); // 음표 머리 생성.
        RectTransform rt = noteHead.GetComponent<RectTransform>(); // 음표 머리 크기 설정.

        rt.anchorMin = new Vector2(0.5f, 0.5f); // 음표 머리 앵커 설정.
        rt.anchorMax = new Vector2(0.5f, 0.5f); // 음표 머리 앵커 설정.
        rt.pivot = new Vector2(0.5f, 0.5f); // 음표 머리 피벗 설정. 
        rt.sizeDelta = new Vector2(noteHeadWidth, noteHeadHeight); // 음표 머리 크기 설정.
        rt.anchoredPosition = new Vector2(0, y); // 음표 머리 위치 설정.    
        rt.localRotation = Quaternion.Euler(0, 0, 45f);  // 기울기 (선택)   
    }

    public void SpawnNoteHeadFromName(string noteName)
{
    if (!noteIndex.ContainsKey(noteName))
    {
        Debug.LogWarning($"🎵 알 수 없는 음표 이름: {noteName}");
        return;
    }

    int index = noteIndex[noteName];    // 예: "C4" → -6
    float spacing = MusicLayoutConfig.GetSpacing(staffPanel);
    float y = index * spacing * 0.5f;  // 음표 머리 위치 설정.

    SpawnNoteHead(y);  // 기존 함수 호출
}
}
