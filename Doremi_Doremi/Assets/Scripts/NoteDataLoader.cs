using UnityEngine;  // UnityEngine 네임스페이스: Debug, TextAsset, JsonUtility 등 제공

/// <summary>
/// NoteDataLoader 클래스는 TextAsset으로 제공된 JSON 악보 데이터를 파싱하여 SongList 객체로 반환합니다.
/// </summary>
public class NoteDataLoader
{
    private TextAsset songsJson;  // JSON 형식으로 된 악보 데이터 파일

    /// <summary>
    /// 생성자: TextAsset 형태의 JSON 데이터를 주입받아 저장합니다.
    /// </summary>
    /// <param name="json">악보 데이터가 포함된 TextAsset</param>
    public NoteDataLoader(TextAsset json)
    {
        songsJson = json;  // 멤버 변수에 JSON 파일 참조 저장
    }

    /// <summary>
    /// JSON 데이터를 SongList 객체로 파싱하여 반환합니다.
    /// JSON 파일이 연결되지 않았을 경우 에러 로그를 출력하고 null을 반환합니다.
    /// </summary>
    /// <returns>파싱된 SongList 객체 또는 null</returns>
    public SongList LoadSongs()
    {
        // JSON 파일이 설정되지 않은 경우 처리
        if (songsJson == null)
        {
            Debug.LogError("[NoteDataLoader] JSON 파일이 없습니다.");  // 에러 로그
            return null;  // null 반환
        }

        // JsonUtility를 사용해 JSON 텍스트를 SongList 클래스로 디시리얼라이즈
        return JsonUtility.FromJson<SongList>(songsJson.text);
    }
}