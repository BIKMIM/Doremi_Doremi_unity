using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NoteEntry
{
    public string pitch;     // 예: "C4"
    public string duration;  // 예: "8", "4.", "2R"
    public int beam = -1;    // beam ID, 없으면 -1
}

[Serializable]
public class Song
{
    public string title;
    public string clef;        // "Treble" 또는 "Bass"
    public string key;         // 조표 (예: "C", "G")
    public string time;        // 박자표 (예: "4/4", "3/4")
    public List<NoteEntry> notes;
}

[Serializable]
public class SongList
{
    public List<Song> songs;
}
