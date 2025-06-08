using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 음표 데이터를 파싱하고 처리하는 유틸리티 클래스
/// - 쉼표 및 마디선 필터링
/// - 음표 이름 정규화
/// - 옥타브 파싱
/// </summary>
public static class NoteDataParser
{
    /// <summary>
    /// 쉼표가 아닌 실제 음표들만 추출
    /// </summary>
    public static List<string> ExtractMusicNotesOnly(List<string> allNotes)
    {
        return allNotes.Where(note => !IsRest(note) && !IsBarLine(note)).ToList();
    }
    
    /// <summary>
    /// 쉼표인지 확인
    /// </summary>
    public static bool IsRest(string noteData)
    {
        if (string.IsNullOrEmpty(noteData)) return false;
        
        string note = noteData.ToLower();
        return note.StartsWith("rest") || note.Contains("rest");
    }
    
    /// <summary>
    /// 마디선이나 기타 기호인지 확인
    /// </summary>
    public static bool IsBarLine(string noteData)
    {
        if (string.IsNullOrEmpty(noteData)) return false;
        
        return noteData == "|" || 
               noteData.Contains("TUPLET") || 
               noteData.Contains("DOUBLE") || 
               noteData.Contains("BAR");
    }
    
    /// <summary>
    /// 음표 데이터에서 음표 이름과 옥타브 파싱
    /// </summary>
    public static (string noteName, int octave) ParseNoteData(string noteData)
    {
        if (string.IsNullOrEmpty(noteData))
            return ("", 4);
        
        // 콜론으로 분리 (예: "C4:quarter" -> "C4")
        string[] parts = noteData.Split(':');
        string notePart = parts[0].Trim();
        
        // 옥타브 파싱
        string noteName = "";
        int octave = 4; // 기본 옥타브
        
        if (notePart.Length >= 2 && char.IsDigit(notePart[notePart.Length - 1]))
        {
            // 마지막 문자가 숫자면 옥타브로 간주
            if (int.TryParse(notePart[notePart.Length - 1].ToString(), out octave))
            {
                noteName = notePart.Substring(0, notePart.Length - 1);
            }
            else
            {
                noteName = notePart;
                octave = 4;
            }
        }
        else
        {
            noteName = notePart;
            octave = 4;
        }
        
        // 음표 이름 정규화
        noteName = NormalizeNoteName(noteName);
        
        return (noteName, octave);
    }
    
    /// <summary>
    /// 음표 이름 정규화 (샤프 기호 통일 등)
    /// </summary>
    public static string NormalizeNoteName(string noteName)
    {
        if (string.IsNullOrEmpty(noteName)) return "";
        
        string normalized = noteName.ToUpper();
        
        // 샤프 기호 정규화
        normalized = normalized.Replace("S", "#")
                              .Replace("SHARP", "#")
                              .Replace("s", "#");
        
        // 플랫 기호도 처리 (필요한 경우)
        normalized = normalized.Replace("FLAT", "b")
                              .Replace("B", "♭"); // 실제 플랫은 ♭ 기호 사용
        
        return normalized;
    }
    
    /// <summary>
    /// 음표 이름을 한국어 계이름으로 변환
    /// </summary>
    public static string GetKoreanNoteName(string noteName)
    {
        return noteName.ToUpper() switch
        {
            "C" => "도(C)",
            "C#" => "도#(C#)",
            "D" => "레(D)",
            "D#" => "레#(D#)",
            "E" => "미(E)",
            "F" => "파(F)",
            "F#" => "파#(F#)",
            "G" => "솔(G)",
            "G#" => "솔#(G#)",
            "A" => "라(A)",
            "A#" => "라#(A#)",
            "B" => "시(B)",
            _ => noteName
        };
    }
    
    /// <summary>
    /// 음표 데이터 유효성 검사
    /// </summary>
    public static bool IsValidNoteData(string noteData)
    {
        if (string.IsNullOrEmpty(noteData)) return false;
        
        var (noteName, octave) = ParseNoteData(noteData);
        
        // 유효한 음표 이름인지 확인
        string[] validNotes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        bool isValidNote = validNotes.Contains(noteName.ToUpper());
        
        // 유효한 옥타브 범위인지 확인 (0~9)
        bool isValidOctave = octave >= 0 && octave <= 9;
        
        return isValidNote && isValidOctave;
    }
    
    /// <summary>
    /// 음표 데이터 배열을 문자열로 변환 (디버깅용)
    /// </summary>
    public static string NotesToString(List<string> notes)
    {
        if (notes == null || notes.Count == 0)
            return "없음";
        
        var parsedNotes = notes.Select(note =>
        {
            var (noteName, octave) = ParseNoteData(note);
            return $"{noteName}{octave}";
        });
        
        return string.Join(" → ", parsedNotes);
    }
    
    /// <summary>
    /// 두 음표가 같은지 비교 (옥타브 무시 옵션)
    /// </summary>
    public static bool CompareNotes(string note1, string note2, bool ignoreOctave = true)
    {
        var (name1, octave1) = ParseNoteData(note1);
        var (name2, octave2) = ParseNoteData(note2);
        
        bool sameNote = name1.ToUpper() == name2.ToUpper();
        
        if (ignoreOctave)
        {
            return sameNote;
        }
        else
        {
            return sameNote && octave1 == octave2;
        }
    }
    
    /// <summary>
    /// 음표 데이터에서 듀레이션 정보 추출 (있는 경우)
    /// </summary>
    public static string GetNoteDuration(string noteData)
    {
        if (string.IsNullOrEmpty(noteData)) return "quarter";
        
        string[] parts = noteData.Split(':');
        if (parts.Length > 1)
        {
            return parts[1].Trim().ToLower();
        }
        
        return "quarter"; // 기본값
    }
    
    /// <summary>
    /// 전체 음표 데이터 분석 및 통계
    /// </summary>
    public static NoteDataStats AnalyzeNoteData(List<string> noteData)
    {
        var stats = new NoteDataStats();
        
        foreach (string note in noteData)
        {
            if (IsRest(note))
            {
                stats.RestCount++;
            }
            else if (IsBarLine(note))
            {
                stats.BarLineCount++;
            }
            else if (IsValidNoteData(note))
            {
                stats.ValidNoteCount++;
            }
            else
            {
                stats.InvalidNoteCount++;
            }
        }
        
        stats.TotalCount = noteData.Count;
        
        return stats;
    }
}

/// <summary>
/// 음표 데이터 분석 결과
/// </summary>
public class NoteDataStats
{
    public int TotalCount { get; set; }
    public int ValidNoteCount { get; set; }
    public int RestCount { get; set; }
    public int BarLineCount { get; set; }
    public int InvalidNoteCount { get; set; }
    
    public override string ToString()
    {
        return $"총 {TotalCount}개 (음표: {ValidNoteCount}, 쉼표: {RestCount}, " +
               $"마디선: {BarLineCount}, 무효: {InvalidNoteCount})";
    }
}
