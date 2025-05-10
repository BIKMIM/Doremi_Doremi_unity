using System.Collections.Generic;
public class NoteMapper
{
    private readonly Dictionary<string, float> noteToIndex = new()
    {
        // 🎼 표준 음역대 (C4-C5)
        // 오선 5줄의 위치는 -2, -1, 0, 1, 2 값을 가짐
        // E4와 F4 사이, G4와 A4 사이가 오선 위에 위치하도록 조정
        { "C4", -4.0f },  // 첫 번째 추가 보조선 아래
        { "D4", -3.5f },  // 첫 번째 보조선과 두 번째 보조선 사이
        { "E4", -3.0f },  // 첫 번째 보조선 위 (첫 번째 오선 아래)
        { "F4", -2.5f },  // 첫 번째 오선과 두 번째 오선 사이
        { "G4", -2.0f },  // 두 번째 오선 위치
        { "A4", -1.5f },  // 두 번째와 세 번째 오선 사이
        { "B4", -1.0f },  // 세 번째 오선 위치
        { "C5", -0.5f },  // 세 번째와 네 번째 오선 사이
        { "D5", 0.0f },   // 네 번째 오선 위치
        { "E5", 0.5f },   // 네 번째와 다섯 번째 오선 사이
        { "F5", 1.0f },   // 다섯 번째 오선 위치
        { "G5", 1.5f },   // 첫 번째 추가 보조선 (오선 위)
        
        // 🎵 샵 음
        { "C4S", -3.8f }, { "D4S", -3.3f }, { "F4S", -2.3f },
        { "G4S", -1.8f }, { "A4S", -1.3f },
        { "C5S", -0.3f }, { "D5S", 0.2f },  { "F5S", 1.2f }
    };

    public bool TryGetIndex(string pitch, out float index)
        => noteToIndex.TryGetValue(pitch, out index);
}