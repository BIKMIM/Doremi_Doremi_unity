using System;
using System.Collections.Generic;


// NoteMapping.cs 파일은 “음표 이름 → 숫자(음정) → 시각적 위치” 로 이어지는 전체 흐름에서,
// 음표의 문자열 이름(예: "C#4" 또는 "Bb3") 을 “보편적인 수치” 로 바꿔 주는 변환 유틸리티 역할
// 왜 MIDI를 거치나?
// MIDI 번호 차이를 이용해 “G4를 기준으로 얼마나 높고 낮은가”를 공식 (midi–67)×0.5 로 계산하면,
// 옥타브가 바뀌어도
// 샾·플랫이 섞여 있어도
// 모두 일관된 간격(0.5칸)으로 오선 위 위치를 산출할 수 있음
// 다양한 옥타브·Accidental 지원을 깔끔하게 하기 위해 MIDI 사용


// 음표 이름을 MIDI 넘버나 오선지 라인 인덱스로 변환해 주는 유틸리티 클래스
public static class NoteMapping
{

    // 음이름(C, C#, Db 등)을 반음 단위 숫자로 매핑하는 사전(딕셔너리)
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

    // 오선지 위에서 기준이 되는 MIDI 넘버 (G4에 해당)
    private const int referenceMidi = 67;            // G4

    // 기준 음(G4)이 오선지에서 차지하는 라인 인덱스 (0으로 설정)
    private const float referenceLineIndex = 0f;     // G4의 lineIndex

    // 반음 한 단계가 오선에서 차지하는 라인 간격 (0.5칸)
    private const float lineSpacingPerSemitone = 0.5f;


    // 음표 이름(예: "C#4")을 받아 오선지 라인 인덱스를 계산하여 반환
    public static float GetLineIndex(string noteName)
    {
        // 음표 이름을 MIDI 숫자로 변환
        int midi = NoteToMidi(noteName);

        // (MIDI 차이) * (라인 간격) + 기준 라인 인덱스
        return (midi - referenceMidi) * lineSpacingPerSemitone + referenceLineIndex;
    }


    // 음표 이름(예: "A3" 또는 "Bb5")을 MIDI 번호로 변환하여 반환
    public static int NoteToMidi(string note)
    {

        // 앞뒤 공백 제거
        note = note.Trim();

        // 피치 부분 추출: 문자(숫자 마지막 자리 제외)
        string pitch = note.Substring(0, note.Length - 1);   // 예: C#, Bb

        // 옥타브 숫자 부분 추출: 마지막 문자
        string octaveStr = note.Substring(note.Length - 1);  // 예: 4


        // 옥타브 문자열을 정수로 파싱, 실패 시 예외 발생
        if (!int.TryParse(octaveStr, out int octave))
            throw new ArgumentException($"Invalid octave in note: {note}");


        // 피치 문자열로 반음 매핑값 조회, 실패 시 예외 발생
        if (!noteToSemitone.TryGetValue(pitch, out int semitone))
            throw new ArgumentException($"Invalid pitch in note: {note}");

        // MIDI 계산 공식: 12 * (옥타브 + 1) + 반음값
        // 예: C4 -> 12*(4+1) + 0 = 60
        return 12 * (octave + 1) + semitone;
    }
}
