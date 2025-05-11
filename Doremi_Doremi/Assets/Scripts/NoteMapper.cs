using System.Collections.Generic;

public class NoteMapper
{
    private readonly Dictionary<string, float> noteToIndex = new()
{
   // 🎵 C4 ~ C5 정확하게 정렬
    { "C4", -0.5f },
{ "D4",  0.0f },
{ "E4",  0.5f },
{ "F4",  1.0f },
{ "G4",  1.5f },
{ "A4",  2.0f },
{ "B4",  2.5f },
{ "C5",  3.0f },
};


    public bool TryGetIndex(string pitch, out float index)
        => noteToIndex.TryGetValue(pitch, out index);
}
