using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 문자열 "C4#:4." → NoteData 객체로 변환
public static class NoteParser
{
    public static NoteData Parse(string noteString)
    {
        NoteData result = new NoteData();
        
        if (string.IsNullOrEmpty(noteString))
        {
            result.isRest = true;
            result.duration = 4;
            return result;
        }

        // 쉼표 처리
        if (noteString.ToUpper().StartsWith("R"))
        {
            result.isRest = true;
            // 기존 쉼표 파싱 로직
            string[] restParts = noteString.Split(':');
            result.duration = 8; // 기본값
            if (restParts.Length > 1)
            {
                string durPart = restParts[1];
                result.isDotted = durPart.EndsWith(".");
                string durVal = result.isDotted ? durPart.TrimEnd('.') : durPart;
                if (int.TryParse(durVal, out int restResult))
                    result.duration = restResult;
            }
            return result;
        }

        // 임시표와 음표명 분리
        string[] parts = noteString.Split(':');
        string noteNamePart = parts[0];
        
        // 🎼 새로운 형식: C4#, D4b, E4n 등으로 임시표 파싱
        result.accidental = ParseAccidental(ref noteNamePart);
        result.noteName = noteNamePart;
        
        // duration과 isDotted 파싱
        result.duration = 8; // 기본값
        if (parts.Length > 1)
        {
            string durPart = parts[1];
            result.isDotted = durPart.EndsWith(".");
            string durVal = result.isDotted ? durPart.TrimEnd('.') : durPart;
            if (int.TryParse(durVal, out int durResult))
                result.duration = durResult;
        }
        
        return result;
    }

    // 🎼 개선된 임시표 파싱 함수 (C4# 형식)
    private static AccidentalType ParseAccidental(ref string noteName)
    {
        AccidentalType accidental = AccidentalType.None;
        
        // 🎯 C4##, C4x (더블샵)
        if (noteName.EndsWith("##") || noteName.EndsWith("x"))
        {
            accidental = AccidentalType.DoubleSharp;
            noteName = noteName.Replace("##", "").Replace("x", "");
        }
        // 🎯 C4bb (더블플랫)
        else if (noteName.EndsWith("bb"))
        {
            accidental = AccidentalType.DoubleFlat;
            noteName = noteName.Replace("bb", "");
        }
        // 🎯 C4n, C4nat (내츄럴)
        else if (noteName.EndsWith("n") || noteName.EndsWith("nat"))
        {
            accidental = AccidentalType.Natural;
            noteName = noteName.Replace("n", "").Replace("nat", "");
        }
        // 🎯 C4# (샵)
        else if (noteName.EndsWith("#"))
        {
            accidental = AccidentalType.Sharp;
            noteName = noteName.Replace("#", "");
        }
        // 🎯 C4b (플랫)
        else if (noteName.EndsWith("b"))
        {
            accidental = AccidentalType.Flat;
            noteName = noteName.Replace("b", "");
        }
        
        return accidental;
    }
}