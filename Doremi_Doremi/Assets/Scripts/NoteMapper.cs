using System.Collections.Generic;

public class NoteMapper
{
    private readonly Dictionary<string, float> noteToIndex = new()
{
   // 🎵 C4 ~ C5 정확하게 정렬
    
        
    { "C3", -4.0f },
    { "D3", -3.5f },
    { "E3", -3.0f },
    { "F3", -2.5f },
    { "G3", -2.0f },
    { "A3", -1.5f },
    { "B3", -1.0f },

    { "C4", -0.5f },
    { "D4",  0.0f },
    { "E4",  0.5f },
    { "F4",  1.0f },
    { "G4",  1.5f },
    { "A4",  2.0f },
    { "B4",  2.5f },

    { "C5",  3.0f },
    { "D5",  3.5f },
    { "E5",  4.0f },
    { "F5",  4.5f },
    { "G5",  5.0f },
    { "A5",  5.5f },
    { "B5",  6.0f },
    
    { "C6",  6.5f },
    { "D6",  7.0f },
    { "E6",  7.5f },
    { "F6",  8.0f },
    { "G6",  8.5f },
    { "A6",  9.0f },
    { "B6",  9.5f },
    { "C7",  10.0f },

};


    public bool TryGetIndex(string pitch, out float index)
        => noteToIndex.TryGetValue(pitch, out index);
}
