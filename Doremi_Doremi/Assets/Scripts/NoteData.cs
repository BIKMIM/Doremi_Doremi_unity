using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 하나의 음표 정보 저장 클래스

public class NoteData
{
    public string noteName;   // 예: "C4"
    public int duration;      // 예: 1, 2, 4, 8, 16
    public bool isDotted;     // 점음표 여부
    public bool isRest;       // 쉼표 여부
    public AccidentalType accidental; // 임시표 타입 (Sharp, Flat, Natural 등)


    public override string ToString() // 음표 정보를 문자열로 변환하는 메서드
    {
        return $"{noteName} | {duration}분음표 | 점음표: {isDotted} | 쉼표: {isRest}"; 
    }
}

