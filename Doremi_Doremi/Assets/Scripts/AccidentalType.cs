using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 임시표 타입 정의
public enum AccidentalType
{
    None,           // 임시표 없음
    Sharp,          // ♯
    Flat,           // ♭
    Natural,        // ♮
    DoubleSharp,    // ♯♯ (x)
    DoubleFlat      // ♭♭
}