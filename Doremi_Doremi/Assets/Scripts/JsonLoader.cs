using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonLoader : MonoBehaviour
{
    [Header("노래파일 연결")]
    public TextAsset songsJson;

    [System.Serializable]
    public class SongData
    {
        public string title;
        public string clef;
        public string timeSignature;
        public string keySignature;
        public List<string> notes;
    }

    [System.Serializable]
    public class SongList
    {
        public List<SongData> songs;
    }

    public SongList LoadSongs()
    {
        Debug.Log("🎼 JsonLoader: LoadSongs 시작");

        // JSON 파일이 연결된 경우 시도
        if (songsJson != null)
        {
            try
            {
                Debug.Log($"🎼 JSON 파일 발견: {songsJson.name}");

                // 먼저 songs 배열 형태로 시도
                try
                {
                    SongList parsed = JsonUtility.FromJson<SongList>(songsJson.text);
                    if (parsed != null && parsed.songs != null && parsed.songs.Count > 0)
                    {
                        Debug.Log($"✅ 배열 형태 JSON에서 {parsed.songs.Count}곡 로딩 완료");
                        return parsed;
                    }
                }
                catch
                {
                    // 배열 형태 실패시 단일 곡 형태로 시도
                    Debug.Log("🎼 단일 곡 형태로 시도합니다.");
                    SongData singleSong = JsonUtility.FromJson<SongData>(songsJson.text);

                    if (singleSong != null && singleSong.notes != null && singleSong.notes.Count > 0)
                    {
                        SongList wrapper = new SongList();
                        wrapper.songs = new List<SongData> { singleSong };

                        Debug.Log($"✅ 단일 곡 JSON에서 1곡 로딩 완료: {singleSong.title}");
                        return wrapper;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ JSON 파싱 에러: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ songs.json 파일이 연결되지 않았습니다.");
        }

        // 모든 경우에 실패했을 때 폴백 테스트 데이터 반환
        Debug.Log("🎼 폴백 테스트 데이터를 사용합니다.");
        return CreateFallbackData();
    }

    private SongList CreateFallbackData()
    {
        SongList testData = new SongList();
        testData.songs = new List<SongData>();

        SongData testSong = new SongData();
        testSong.title = "Fallback Test Song";
        testSong.clef = "treble";
        testSong.timeSignature = "4/4";
        testSong.keySignature = "";
        testSong.notes = new List<string> { "C4#:4", "D4b:4", "E4n:4", "F4##:4", "G4bb:4", "A4x:4", "B4:4" };

        testData.songs.Add(testSong);

        Debug.Log($"✅ 폴백 데이터 생성 완료 - {testData.songs.Count}곡");
        return testData;
    }
}