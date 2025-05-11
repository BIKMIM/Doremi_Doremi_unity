using System;

/// <summary>
/// ���� �� ������ ��� ������ Ŭ�����Դϴ�.
/// Title�� Note �迭�� �����Ͽ� �ϳ��� �Ǻ� ������ ǥ���մϴ�.
/// </summary>
[Serializable]
public class Song
{
    /// <summary>
    /// �� ���� (��: "Twinkle Twinkle Little Star")
    /// </summary>
    public string title;

    /// <summary>
    /// ���� �����ϴ� ��ǥ ���ڿ� �迭
    /// ��: { "C4:4", "D4:4", "E4:2R" }
    /// </summary>
    public string[] notes;

    public string clef;
}

/// <summary>
/// Song ��ü�� �迭�� ���� JSON ��ø�������� �� ���Ǵ� ���� Ŭ�����Դϴ�.
/// SongList.songs�� ���� ���� ���� �����մϴ�.
/// </summary>
[Serializable]
public class SongList
{
    /// <summary>
    /// �� ��� �迭
    /// JSON Ű�� "songs"�� ���ε˴ϴ�.
    /// </summary>
    public Song[] songs;
}