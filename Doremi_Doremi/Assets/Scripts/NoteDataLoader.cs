using UnityEngine;

/// <summary>
/// TextAsset으로 제공된 JSON 악보 데이터를 SongList로 파싱합니다.
/// </summary>
public class NoteDataLoader
{
    private TextAsset songsJson;

    public NoteDataLoader(TextAsset json)
    {
        songsJson = json;
    }

    /// <summary>
    /// JsonUtility를 이용해 SongList 객체로 디시리얼라이즈합니다.
    /// </summary>
    public SongList LoadSongs()
    {
        if (songsJson == null)
        {
            Debug.LogError("[NoteDataLoader] JSON 파일이 없습니다.");
            return null;
        }
        return JsonUtility.FromJson<SongList>(songsJson.text);
    }
}
