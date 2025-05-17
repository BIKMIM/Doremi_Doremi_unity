using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonLoader : MonoBehaviour
{
    [System.Serializable]
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

    [Header("노래파일 연결 songs.json")]
    public TextAsset songsJson;



    public SongList LoadSongs()
    {
        if (songsJson == null)
        {
            Debug.LogError("❗ JSON 파일이 연결되지 않았습니다.");
            return null;
        }

        SongList parsed = JsonUtility.FromJson<SongList>(songsJson.text);

        if (parsed == null || parsed.songs == null || parsed.songs.Count == 0)
        {
            Debug.LogWarning("⚠️ 노래가 없거나 JSON 구조가 잘못되었습니다.");
            return null;
        }

        Debug.Log($"✅ 총 {parsed.songs.Count}곡 로딩 완료");
        return parsed;
    }
}
