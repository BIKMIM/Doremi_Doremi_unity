using System;

[Serializable]
public class Song
{
    public string title;
    public string[] notes;
}

[Serializable]
public class SongList
{
    public Song[] songs;
}
