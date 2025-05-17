using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonLoader : MonoBehaviour
{

    [Header("노래파일 연결 songs.json")] // 인스펙터에 메뉴 생성
    public TextAsset songsJson;


    [System.Serializable] // JSON 데이터같은 연속된 DATA 구조를 정의하는 클래스
    public class SongData 
    {
        public string title; 
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
        return parsed;
    }
}
