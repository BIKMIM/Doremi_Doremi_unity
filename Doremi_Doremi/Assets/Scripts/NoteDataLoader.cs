using UnityEngine;

public class NoteDataLoader
{
    private TextAsset songsJson;

    public NoteDataLoader(TextAsset json)
    {
        songsJson = json;
    }

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
