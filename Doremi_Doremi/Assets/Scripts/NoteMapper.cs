using System.Collections.Generic;

public class NoteMapper
{
    // 🎯 Treble Clef 조표 위치
    private static readonly Dictionary<string, float> trebleKeySigIndex = new()
    {
        { "F#", -1.0f }, { "C#", -2.5f }, { "G#", -0.5f }, { "D#", -2.0f },
        { "A#", -3.5f }, { "E#", -1.5f }, { "B#", -3f },
        { "Bb", -2.7f }, { "Eb", -1.0f }, { "Ab", -3f },
        { "Db", -1.7f }, { "Gb", -3.5f }, { "Cb", -2.0f }, { "Fb", -4.0f }
    };

    // ✅ 조표 위치 반환 (Treble 기준)
    public static float GetKeySignatureIndex(string accidentalNote)
    {
        return trebleKeySigIndex.TryGetValue(accidentalNote, out float index) ? index : 0f;
    }

    // ✅ 음표 위치 반환 (C4, G#4 등)
    public bool TryGetIndex(string pitch, out float index)
    {
        // 1) 자연음 처리
        if (noteToIndex.TryGetValue(pitch, out index))
            return true;

        // 2) 샵/플랫 처리
        if (pitch.Length >= 3 && (pitch[1] == '#' || pitch[1] == 'b'))
        {
            string letter = pitch.Substring(0, 1);
            string accidental = pitch.Substring(1, 1);
            string octave = pitch.Substring(2);
            string baseNote = letter + octave;

            if (noteToIndex.TryGetValue(baseNote, out float baseIndex))
            {
                index = baseIndex + (accidental == "#" ? 0.5f : -0.5f);
                return true;
            }
        }

        index = 0;
        return false;
    }

    // 🎵 오선 음높이 기준값
    private readonly Dictionary<string, float> noteToIndex = new()
    {
        { "C3", -4.0f }, { "D3", -3.5f }, { "E3", -3.0f }, { "F3", -2.5f },
        { "G3", -2.0f }, { "A3", -1.5f }, { "B3", -1.0f }, { "C4", -0.5f },
        { "D4",  0.0f }, { "E4",  0.5f }, { "F4",  1.0f }, { "G4",  1.5f },
        { "A4",  2.0f }, { "B4",  2.5f }, { "C5",  3.0f }, { "D5",  3.5f },
        { "E5",  4.0f }, { "F5",  4.5f }, { "G5",  5.0f }, { "A5",  5.5f },
        { "B5",  6.0f }, { "C6",  6.5f }, { "D6",  7.0f }, { "E6",  7.5f },
        { "F6",  8.0f }, { "G6",  8.5f }, { "A6",  9.0f }, { "B6",  9.5f },
        { "C7",  10.0f }
    };
}
