using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON 형태의 악보 데이터를 로드하는 클래스
/// - songs.json 파일에서 곡 정보를 읽어옴
/// - 자동으로 Resources 폴더에서 파일 검색
/// - 음자리표, 박자표, 조표 정보 포함
/// </summary>
public class JsonLoader : MonoBehaviour
{
    [Header("노래파일 연결 songs.json")]
    public TextAsset songsJson;

    [System.Serializable]
    public class SongData
    {
        public string title;
        public string clef; // 음자리표 정보 (treble, bass)
        public string timeSignature; // 박자 정보
        public string keySignature; // 조표 정보
        public List<string> notes;
    }

    [System.Serializable]
    public class SongList
    {
        public List<SongData> songs;
    }

    void Start()
    {
        Debug.Log("🔍 JsonLoader Start() 실행됨");

        // Resources 폴더에서 자동으로 songs.json 로드
        if (songsJson == null)
        {
            Debug.Log("🔍 songsJson이 null이므로 Resources에서 로드 시도");
            LoadSongsFromResources();
        }
        else
        {
            Debug.Log("🔍 songsJson이 이미 할당되어 있음: " + songsJson.name);
        }
    }

    /// <summary>
    /// Resources 폴더에서 songs.json을 자동으로 로드
    /// </summary>
    private void LoadSongsFromResources()
    {
        Debug.Log("🔍 LoadSongsFromResources() 실행");

        // Resources 폴더의 모든 텍스트 파일 확인
        TextAsset[] allTextAssets = Resources.LoadAll<TextAsset>("");
        Debug.Log($"🔍 Resources 폴더에서 찾은 TextAsset 파일 수: {allTextAssets.Length}");

        foreach (var asset in allTextAssets)
        {
            Debug.Log($"🔍 발견된 파일: {asset.name}");
        }

        songsJson = Resources.Load<TextAsset>("songs");

        if (songsJson == null)
        {
            Debug.LogError("❗ Resources/songs.json 파일을 찾을 수 없습니다. 파일 경로를 확인해주세요.");
            Debug.LogError("📁 올바른 경로: Assets/Resources/songs.json");

            // 다른 경로도 시도해보기
            songsJson = Resources.Load<TextAsset>("Songs/songs");
            if (songsJson != null)
            {
                Debug.Log("✅ Songs/songs 경로에서 발견!");
            }
        }
        else
        {
            Debug.Log("✅ Resources에서 songs.json을 성공적으로 로드했습니다.");
            Debug.Log($"📄 파일 내용 미리보기: {songsJson.text.Substring(0, Mathf.Min(100, songsJson.text.Length))}...");
        }
    }

    /// <summary>
    /// 노래 목록을 로드하는 메인 메서드
    /// </summary>
    public SongList LoadSongs()
    {
        Debug.Log("🔍 LoadSongs() 실행");

        // songsJson이 없으면 Resources에서 다시 시도
        if (songsJson == null)
        {
            Debug.Log("🔍 songsJson이 null이므로 다시 로드 시도");
            LoadSongsFromResources();
        }

        if (songsJson == null)
        {
            Debug.LogError("❗ JSON 파일이 연결되지 않았습니다.");
            Debug.LogError("💡 해결방법:");
            Debug.LogError("1. Assets/Resources/songs.json 파일이 존재하는지 확인");
            Debug.LogError("2. Inspector에서 수동으로 songs.json 파일을 할당");
            return null;
        }

        Debug.Log($"🔍 JSON 파싱 시도, 내용 길이: {songsJson.text.Length}");

        SongList parsed = JsonUtility.FromJson<SongList>(songsJson.text);

        if (parsed == null || parsed.songs == null || parsed.songs.Count == 0)
        {
            Debug.LogWarning("⚠️ 노래가 없거나 JSON 구조가 잘못되었습니다.");
            Debug.LogWarning("📝 JSON 내용:");
            Debug.LogWarning(songsJson.text);
            return null;
        }

        Debug.Log($"✅ 총 {parsed.songs.Count}곡 로딩 완료");

        // 각 곡의 정보 로그 출력
        foreach (var song in parsed.songs)
        {
            string clefType = string.IsNullOrEmpty(song.clef) ? "treble (기본값)" : song.clef;
            Debug.Log($"🎵 {song.title}: {clefType} clef, {song.timeSignature}, {song.notes.Count}개 음표");
        }

        return parsed;
    }

    // === 디버그 메서드들 ===

    [ContextMenu("테스트: 노래 로드")]
    public void TestLoadSongs()
    {
        LoadSongs();
    }

    [ContextMenu("디버그: Resources 폴더 확인")]
    public void DebugResourcesFolder()
    {
        TextAsset[] allFiles = Resources.LoadAll<TextAsset>("");
        Debug.Log($"Resources 폴더의 텍스트 파일 개수: {allFiles.Length}");

        foreach (var file in allFiles)
        {
            Debug.Log($"파일명: {file.name}, 크기: {file.text.Length}글자");
        }
    }
}
