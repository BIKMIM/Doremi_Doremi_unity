using System;

/// <summary>
/// 단일 곡 정보를 담는 데이터 클래스입니다.
/// Title과 Note 배열을 포함하여 하나의 악보 단위를 표현합니다.
/// </summary>
[Serializable]
public class Song
{
    /// <summary>
    /// 곡 제목 (예: "Twinkle Twinkle Little Star")
    /// </summary>
    public string title;

    /// <summary>
    /// 곡을 구성하는 음표 문자열 배열
    /// 예: { "C4:4", "D4:4", "E4:2R" }
    /// </summary>
    public string[] notes;

    public string clef;

    public string time; 

    public string key;   
}

/// <summary>
/// Song 객체를 배열로 묶어 JSON 디시리얼라이즈 시 사용되는 래퍼 클래스입니다.
/// SongList.songs를 통해 여러 곡을 관리합니다.
/// </summary>
[Serializable]
public class SongList
{
    /// <summary>
    /// 곡 목록 배열
    /// JSON 키는 "songs"로 매핑됩니다.
    /// </summary>
    public Song[] songs;
}