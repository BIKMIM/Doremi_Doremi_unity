using System.Collections.Generic;

// NoteIndexTable.cs - 음표 이름과 인덱스 매핑 테이블
public static class NoteIndexTable
{
    public static readonly Dictionary<string, float> Table = new Dictionary<string, float>
    {
        // 🎼 C3 옥타브 (인덱스 -13 ~ -7)
        { "C3", -13f},   { "D3", -12f},   { "E3", -11f},   { "F3", -10f},   
        { "G3", -9f},    { "A3", -8f},    { "B3", -7f},

        // 🎼 C4 옥타브 (인덱스 -6 ~ 0)
        { "C4", -6f},    { "D4", -5f},    { "E4", -4f},    { "F4", -3f},    
        { "G4", -2f},    { "A4", -1f},    { "B4", 0f},

        // 🎼 C5 옥타브 (인덱스 1 ~ 7)
        { "C5", 1f},     { "D5", 2f},     { "E5", 3f},     { "F5", 4f},     
        { "G5", 5f},     { "A5", 6f},     { "B5", 7f},

        // 🎼 C6 옥타브 (인덱스 8 ~ 14)
        { "C6", 8f},     { "D6", 9f},     { "E6", 10f},    { "F6", 11f},    
        { "G6", 12f},    { "A6", 13f},    { "B6", 14f},

        // 🎼 C2 옥타브 (인덱스 -20 ~ -14)
        { "C2", -20f},   { "D2", -19f},   { "E2", -18f},   { "F2", -17f},   
        { "G2", -16f},   { "A2", -15f},   { "B2", -14f},

        // 🎼 C7 옥타브 (인덱스 15 ~ 21)
        { "C7", 15f},    { "D7", 16f},    { "E7", 17f},    { "F7", 18f},    
        { "G7", 19f},    { "A7", 20f},    { "B7", 21f}
    };

    // 🎼 오선 위의 음표들 (덧줄이 필요 없는 음표)
    public static readonly HashSet<string> LineNotes = new HashSet<string>
    {
        // 오선 5줄
        "F5", "D5", "B4", "G4", "E4",
        
        // 오선 아래 덧줄들
        "C4", "A3", "G3", "E3", "C3", "A2", "G2", "E2", "C2",
        
        // 오선 위 덧줄들
        "A5", "C6", "E6", "G6", "B6", "D7", "F7", "A7"
    };

    public static bool ContainsKey(string noteName)
    {
        return Table.ContainsKey(noteName);
    }

    public static float GetIndex(string noteName)
    {
        return Table.ContainsKey(noteName) ? Table[noteName] : 0f;
    }

    public static bool IsOnLine(string noteName)
    {
        return LineNotes.Contains(noteName);
    }
}