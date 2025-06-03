using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public static class NoteParser
{
    private static readonly Dictionary<string, AccidentalType> accidentalPatterns = new Dictionary<string, AccidentalType>
    {
        { "##", AccidentalType.DoubleSharp },
        { "x", AccidentalType.DoubleSharp },
        { "bb", AccidentalType.DoubleFlat },
        { "#", AccidentalType.Sharp },
        { "b", AccidentalType.Flat },
        { "n", AccidentalType.Natural }
    };

    public static NoteData Parse(string raw)   
    {
        var data = new NoteData(); 

        data.isRest = raw.StartsWith("R") || raw.StartsWith("r");
        
        string[] parts = raw.Split(':'); 
        string noteNamePart = parts[0];
        
        // 임시표 파싱
        data.accidental = ParseAccidental(ref noteNamePart);
        data.noteName = noteNamePart;
        
        data.duration = 8; // 기본값

        if (parts.Length > 1) 
        {
            string durPart = parts[1];
            data.isDotted = durPart.EndsWith(".");
            string durVal = data.isDotted ? durPart.TrimEnd('.') : durPart;
            if (int.TryParse(durVal, out int result))
                data.duration = result; 
        }

        Debug.Log($"파싱 결과: {data.noteName} | 길이:{data.duration} | 점음표:{data.isDotted} | 쉼표:{data.isRest} | 임시표:{data.accidental}");
        
        return data;
    }

    private static AccidentalType ParseAccidental(ref string noteName)
    {
        if (string.IsNullOrEmpty(noteName))
            return AccidentalType.None;

        // 더블샵과 더블플랫을 먼저 확인 (더 긴 패턴을 우선 처리)
        foreach (var pattern in accidentalPatterns)
        {
            if (noteName.Contains(pattern.Key))
            {
                noteName = noteName.Replace(pattern.Key, "");
                Debug.Log($"임시표 발견: {pattern.Key} -> {pattern.Value}");
                return pattern.Value;
            }
        }

        return AccidentalType.None;
    }

    public static NoteData ParseAdvanced(string raw)
    {
        var data = new NoteData();

        string pattern = @"^(R|r)?([A-Ga-g])([#b nx]*)(\d)?(:(\d+)(\.)?)?$";
        Match match = Regex.Match(raw, pattern);

        if (!match.Success)
        {
            Debug.LogWarning($"파싱 실패: {raw}. 기본 파서를 사용합니다.");
            return Parse(raw);
        }

        data.isRest = !string.IsNullOrEmpty(match.Groups[1].Value);
        string baseNote = match.Groups[2].Value.ToUpper();
        string accidentalString = match.Groups[3].Value;
        string octave = match.Groups[4].Value;
        string durationString = match.Groups[6].Value;
        data.isDotted = !string.IsNullOrEmpty(match.Groups[7].Value);

        data.noteName = baseNote + (string.IsNullOrEmpty(octave) ? "4" : octave);
        data.accidental = ParseAccidentalFromString(accidentalString);

        if (!string.IsNullOrEmpty(durationString) && int.TryParse(durationString, out int duration))
        {
            data.duration = duration;
        }
        else
        {
            data.duration = 8;
        }

        Debug.Log($"고급 파싱 결과: {data.noteName} | 길이:{data.duration} | 점음표:{data.isDotted} | 쉼표:{data.isRest} | 임시표:{data.accidental}");
        
        return data;
    }

    private static AccidentalType ParseAccidentalFromString(string accidentalString)
    {
        if (string.IsNullOrEmpty(accidentalString))
            return AccidentalType.None;

        var sortedPatterns = new List<KeyValuePair<string, AccidentalType>>(accidentalPatterns);
        sortedPatterns.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));

        foreach (var pattern in sortedPatterns)
        {
            if (accidentalString.Contains(pattern.Key))
            {
                return pattern.Value;
            }
        }

        Debug.LogWarning($"알 수 없는 임시표: {accidentalString}");
        return AccidentalType.None;
    }

    public static string NormalizeNoteString(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return raw;

        return raw.Trim();
    }

    public static bool SupportsAccidentals()
    {
        return true;
    }

    public static string[] GetSupportedAccidentals()
    {
        return new string[] { "#", "b", "n", "##", "bb", "x" };
    }
}