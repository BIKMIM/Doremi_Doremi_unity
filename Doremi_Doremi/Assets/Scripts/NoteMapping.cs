using System;
using System.Collections.Generic;

public static class NoteMapping
{
    private static readonly Dictionary<string, int> noteToSemitone = new()
    {
        { "C", 0 }, { "C#", 1 }, { "Db", 1 },
        { "D", 2 }, { "D#", 3 }, { "Eb", 3 },
        { "E", 4 },
        { "F", 5 }, { "F#", 6 }, { "Gb", 6 },
        { "G", 7 }, { "G#", 8 }, { "Ab", 8 },
        { "A", 9 }, { "A#", 10 }, { "Bb", 10 },
        { "B", 11 }
    };

    private const int referenceMidi = 67;            // G4
    private const float referenceLineIndex = 0f;     // G4ÀÇ lineIndex
    private const float lineSpacingPerSemitone = 0.5f;

    public static float GetLineIndex(string noteName)
    {
        int midi = NoteToMidi(noteName);
        return (midi - referenceMidi) * lineSpacingPerSemitone + referenceLineIndex;
    }

    public static int NoteToMidi(string note)
    {
        note = note.Trim();
        string pitch = note.Substring(0, note.Length - 1);   // ¿¹: C#, Bb
        string octaveStr = note.Substring(note.Length - 1);  // ¿¹: 4

        if (!int.TryParse(octaveStr, out int octave))
            throw new ArgumentException($"Invalid octave in note: {note}");

        if (!noteToSemitone.TryGetValue(pitch, out int semitone))
            throw new ArgumentException($"Invalid pitch in note: {note}");

        return 12 * (octave + 1) + semitone;
    }
}
