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

    // 🎵 오선 음높이 기준값 - C4부터 시작
    private readonly Dictionary<string, float> _noteToIndex = new()
    {
        { "A3", -3.0f },   // 첫 번째 보조선 아래 2칸
        { "B3", -2.5f },   // 첫 번째 보조선 아래 1칸
        { "C4", -2.0f },   // 첫 번째 보조선
        { "D4", -1.5f },   // 보조선과 첫 줄 사이
        { "E4", 0.0f },    // 첫 번째 줄
        { "F4", 0.5f },    // 첫~두 번째 줄 사이
        { "G4", 1.0f },    // 두 번째 줄
        { "A4", 1.5f },    // 두~세 번째 줄 사이
        { "B4", 2.0f },    // 세 번째 줄
        { "C5", 2.5f },    // 세~네 번째 줄 사이
        { "D5", 3.0f },    // 네 번째 줄
        { "E5", 3.5f },    // 네~다섯 번째 줄 사이
        { "F5", 4.0f },    // 다섯 번째 줄
        { "G5", 4.5f },    // 다섯 번째 줄 위
        { "A5", 5.0f },    // 다섯 번째 줄 위 칸
        { "B5", 5.5f },    // 다섯 번째 줄 위 두 칸
        { "C6", 6.0f },    // 다섯 번째 줄 위 세 칸
    };

    public bool TryGetIndex(string note, out float index)
    {
        return _noteToIndex.TryGetValue(note, out index);
    }
}
