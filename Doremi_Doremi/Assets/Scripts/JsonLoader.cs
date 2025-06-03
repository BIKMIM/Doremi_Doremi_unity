using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// JsonLoader.cs - 노래를 저장한 Json 악보를 불러오기 위한 파일

public class JsonLoader : MonoBehaviour
{

    [Header("노래파일 연결 songs.json")] // 인스펙터에 메뉴 생성
    public TextAsset songsJson;


    [System.Serializable] // JSON 데이터같은 연속된 DATA 구조를 정의하는 클래스
    public class SongData
    {
        public string title;
        public string clef; // 🎼 음자리표 정보 추가 (treble, bass)
        public string timeSignature; // 곡의 박자 정보를 담을 변수
        public string keySignature;  // ← 이 줄 추가
        public List<string> notes;
    }

    [System.Serializable]
    public class SongList
    {
        public List<SongData> songs;
    }


    public SongList LoadSongs() // 노래 목록을 로드하는 메서드
    {
        if (songsJson == null) // JSON 파일이 연결되지 않았을 경우
        {
            Debug.LogError("❗ JSON 파일이 연결되지 않았습니다.");
            return null;
        }

        SongList parsed = JsonUtility.FromJson<SongList>(songsJson.text); // JSON 파일을 파싱하여 SongList 객체로 변환

        if (parsed == null || parsed.songs == null || parsed.songs.Count == 0)  // 파싱된 데이터가 없거나 곡 목록이 비어있을 경우
        {
            Debug.LogWarning("⚠️ 노래가 없거나 JSON 구조가 잘못되었습니다.");
            return null;
        }

        Debug.Log($"✅ 총 {parsed.songs.Count}곡 로딩 완료");

        // 🎼 각 곡의 음자리표 정보 로그 출력
        foreach (var song in parsed.songs)
        {
            string clefType = string.IsNullOrEmpty(song.clef) ? "treble (기본값)" : song.clef;
            Debug.Log($"🎵 {song.title}: {clefType} clef, {song.timeSignature}, {song.notes.Count}개 음표");
        }

        return parsed;
    }
}