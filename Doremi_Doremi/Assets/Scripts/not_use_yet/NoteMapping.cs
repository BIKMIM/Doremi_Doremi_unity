using System;  // 기본 시스템 네임스페이스
using System.Collections.Generic;  // Dictionary 등 컨테이너 사용

/// <summary>
/// NoteMapping 클래스는 음표 이름(예: "C4")을 MIDI 번호로 변환하고,
/// 해당 MIDI를 기준으로 오선 위에서의 lineIndex(세로 위치)를 계산하는 유틸리티입니다.
/// </summary>
public static class NoteMapping
{
    // 음이름을 반음(semitone)으로 매핑하는 Dictionary
    private static readonly Dictionary<string, int> noteToSemitone = new()
    {
        { "C", 0 }, { "C#", 1 }, { "Db", 1 },  // C 계열
        { "D", 2 }, { "D#", 3 }, { "Eb", 3 },  // D 계열
        { "E", 4 },                               // E
        { "F", 5 }, { "F#", 6 }, { "Gb", 6 },  // F 계열
        { "G", 7 }, { "G#", 8 }, { "Ab", 8 },  // G 계열
        { "A", 9 }, { "A#", 10 }, { "Bb", 10 },// A 계열
        { "B", 11 }                               // B
    };

    private const int referenceMidi = 67;            // 기준 음: G4의 MIDI 번호 (G4 = 67)
    private const float referenceLineIndex = 0f;     // 기준 음(G4)이 배치될 lineIndex
    private const float lineSpacingPerSemitone = 0.5f; // 반음마다 움직일 lineIndex 간격

    /// <summary>
    /// 음표 이름(noteName)에 대응하는 lineIndex를 반환합니다.
    /// MIDI 번호 차이를 이용해 오선 기준 오프셋을 계산합니다.
    /// </summary>
    /// <param name="noteName">예: "C4", "D#5"</param>
    /// <returns>오선 위에서의 상대적 위치 인덱스</returns>
    public static float GetLineIndex(string noteName)
    {
        // 음표 이름 → MIDI 번호 변환
        int midi = NoteToMidi(noteName);
        // 기준 MIDI와 비교하여 간격을 계산하고, semitone당 0.5 라인 이동 적용
        return (midi - referenceMidi) * lineSpacingPerSemitone + referenceLineIndex;
    }

    /// <summary>
    /// 음표 문자열(예: "C#4")을 MIDI 번호로 변환합니다.
    /// </summary>
    /// <param name="note">음표 문자열</param>
    /// <returns>MIDI 번호(int)</returns>
    /// <exception cref="ArgumentException">잘못된 옥타브 또는 피치일 때 예외 발생</exception>
    public static int NoteToMidi(string note)
    {
        note = note.Trim();  // 앞뒤 공백 제거
        // 마지막 한 글자는 옥타브 숫자(예: '4'), 나머지는 피치(예: "C#")
        string pitch = note.Substring(0, note.Length - 1);
        string octaveStr = note.Substring(note.Length - 1);

        // 옥타브 문자열을 정수로 변환
        if (!int.TryParse(octaveStr, out int octave))
            throw new ArgumentException($"Invalid octave in note: {note}");

        // 피치 문자열을 semitone 값으로 조회
        if (!noteToSemitone.TryGetValue(pitch, out int semitone))
            throw new ArgumentException($"Invalid pitch in note: {note}");

        // MIDI 번호 계산식: 옥타브 * 12 + semitone, 단 옥타브 0이 C-1을 가리키므로 +1 보정
        return 12 * (octave + 1) + semitone;
    }
}
