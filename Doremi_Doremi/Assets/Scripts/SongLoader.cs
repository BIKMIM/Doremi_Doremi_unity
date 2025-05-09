using UnityEngine;
using System;

public class SongLoader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject trebleClef;  // 높은음자리표
    public GameObject bassClef;    // 낮은음자리표
    public NoteSpawner noteSpawner; // NoteSpawner 참조

    [Serializable]
    public class SongData
    {
        public string title;  // 곡 제목
        public NoteSpawner.ClefType clef;  // 음자리표 (높은음자리표, 낮은음자리표)
        public string[] measures;  // 마디 정보 (음표, 쉼표 등)
    }

    [Serializable]
    public class SongList
    {
        public SongData[] songs;  // 곡 리스트
    }

    void Start()
    {
        LoadSongFromJson("Songs/song_list");
    }

    // 🎵 JSON에서 곡을 읽고 로드하는 함수
    void LoadSongFromJson(string path)
    {
        // JSON 파일을 불러오기
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        if (jsonFile == null)
        {
            Debug.LogError($"❌ JSON 파일을 찾을 수 없습니다: Resources/{path}.json");
            return;
        }

        // JSON 파싱
        SongList songList = JsonUtility.FromJson<SongList>(jsonFile.text);
        if (songList == null || songList.songs == null || songList.songs.Length == 0)
        {
            Debug.LogError("❌ JSON에 유효한 노래 정보가 없습니다.");
            return;
        }

        // 첫 번째 곡을 로드
        Debug.Log($"🎶 첫 번째 곡 로드 완료: {songList.songs[0].title}");
        PlaySong(songList.songs[0]);
    }

    // 🎼 곡을 로드하고 음자리표를 설정한 후 노래를 시작하는 함수
    void PlaySong(SongData song)
    {
        Debug.Log($"🎼 곡 시작: {song.title}, Clef: {song.clef}");

        // 음자리표 설정 (Treble 또는 Bass)
        trebleClef.SetActive(song.clef == NoteSpawner.ClefType.Treble);
        bassClef.SetActive(song.clef == NoteSpawner.ClefType.Bass);

        // noteSpawner에서 음표 그리기
        noteSpawner.LoadSong(song);
    }

    // 🎵 곡을 로드하고 음표 생성
    public void LoadSong(SongData song)
    {
        noteSpawner.clefType = song.clef;

        // 곡의 음표 처리
        for (int m = 0; m < song.measures.Length; m++)
        {
            string measure = song.measures[m]; // 마디 처리
            string[] noteStrings = measure.Split(',');
            for (int i = 0; i < noteStrings.Length; i++)
            {
                string note = noteStrings[i].Trim();  // 음표가 문자열로 전달되므로 Trim()으로 공백 제거
                float noteValue = noteSpawner.ConvertNoteToFloat(note);  // 실수로 변환

                // 음표를 생성하고 위치 설정
                noteSpawner.SpawnNote(noteValue, m * 100f);  // X 위치와 Y 위치 계산
            }
        }
    }
}
